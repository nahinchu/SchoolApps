using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace SchoolApp.Hubs
{
    public class ChessHub : Hub
    {
        // Lưu phòng trong RAM. Dùng ConcurrentDictionary vì nhiều kết nối
        // có thể vào/thoát đồng thời. App chạy 1 container nên không cần Redis.
        private static readonly ConcurrentDictionary<string, Room> Rooms = new();

        private readonly ILogger<ChessHub> _log;
        public ChessHub(ILogger<ChessHub> log) => _log = log;

        private class Room
        {
            public string White { get; set; } = "";
            public string? Black { get; set; }
            // Nước vừa relay gần nhất — dùng để chặn echo.
            public int LastFrom { get; set; } = -1;
            public int LastTo { get; set; } = -1;
        }

        public async Task CreateRoom()
        {
            string code = Guid.NewGuid().ToString()[..4].ToUpper();
            Rooms[code] = new Room { White = Context.ConnectionId };
            await Groups.AddToGroupAsync(Context.ConnectionId, code);
            _log.LogInformation("[CHESS] CreateRoom code={Code} white={Conn}", code, Context.ConnectionId);
            await Clients.Caller.SendAsync("RoomCreated", code);
        }

        public async Task JoinRoom(string code)
        {
            code = (code ?? "").ToUpper();
            if (!Rooms.TryGetValue(code, out var room) || room.Black != null)
            {
                _log.LogWarning("[CHESS] JoinRoom FAIL code={Code} (không tồn tại hoặc đã đầy)", code);
                await Clients.Caller.SendAsync("Error", "Phòng không tồn tại hoặc đã đầy");
                return;
            }
            room.Black = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, code);
            _log.LogInformation("[CHESS] JoinRoom OK code={Code} black={Conn} -> GameStart", code, Context.ConnectionId);
            await Clients.Client(room.White).SendAsync("GameStart", "white");
            await Clients.Caller.SendAsync("GameStart", "black");
        }

        public async Task Resign()
        {
            var entry = Rooms.FirstOrDefault(r =>
                r.Value.White == Context.ConnectionId || r.Value.Black == Context.ConnectionId);
            if (entry.Key == null) return;
            _log.LogInformation("[CHESS] Resign by={Conn} room={Code}", Context.ConnectionId, entry.Key);
            await Clients.OthersInGroup(entry.Key).SendAsync("OpponentResigned");
        }

        public async Task SendMove(string code, int from, int to, int promotion)
        {
            // KHÔNG tin 'code' từ client (Black hay gửi rỗng). Tự tra phòng theo
            // ConnectionId của người gửi.
            var entry = Rooms.FirstOrDefault(r =>
                r.Value.White == Context.ConnectionId || r.Value.Black == Context.ConnectionId);
            var roomCode = entry.Key;
            var room = entry.Value;
            if (roomCode == null || room == null)
            {
                _log.LogWarning("[CHESS] SendMove bỏ qua: không thấy phòng cho {Conn}", Context.ConnectionId);
                return;
            }

            // Chặn echo: nước trùng y hệt nước vừa relay (client áp dụng nước đối
            // thủ rồi vô tình gửi lại). Không thể có nước hợp lệ trùng cả from+to
            // ngay sau đó nên bỏ qua an toàn.
            if (from == room.LastFrom && to == room.LastTo)
            {
                _log.LogInformation("[CHESS] Bỏ echo from={From} to={To} room={Code}", from, to, roomCode);
                return;
            }
            room.LastFrom = from;
            room.LastTo = to;

            _log.LogInformation("[CHESS] SendMove room={Code} from={From} to={To} promo={Promo} by={Conn}",
                roomCode, from, to, promotion, Context.ConnectionId);
            await Clients.OthersInGroup(roomCode).SendAsync("OpponentMove", from, to, promotion);
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var entry = Rooms.FirstOrDefault(r =>
                r.Value.White == Context.ConnectionId ||
                r.Value.Black == Context.ConnectionId);

            if (entry.Key != null)
            {
                await Clients.OthersInGroup(entry.Key).SendAsync("OpponentDisconnected");
                Rooms.TryRemove(entry.Key, out _);
            }
            await base.OnDisconnectedAsync(ex);
        }
    }
}