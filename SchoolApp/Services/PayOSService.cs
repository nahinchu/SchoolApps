using SchoolApp.DTOs;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SchoolApp.Services
{
    public class PayOSService
    {
        private readonly HttpClient _http;
        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private const string BaseUrl = "https://api-merchant.payos.vn";

        public PayOSService(HttpClient http, IConfiguration config)
        {
            _http = http;
            var section = config.GetSection("PayOS");
            _clientId = section["ClientId"]!;
            _apiKey = section["ApiKey"]!;
            _checksumKey = section["ChecksumKey"]!;
        }
            
        private string SignPayload(string data)
        {
            var key = Encoding.UTF8.GetBytes(_checksumKey);
            var msg = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA256(key);
            return Convert.ToHexString(hmac.ComputeHash(msg)).ToLower();
        }

        private string BuildSignatureString(long orderCode, int amount, string description, string cancelUrl, string returnUrl)
        {
            return $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
        }

        public async Task<PayOSCreateResponse> CreatePaymentLinkAsync(
            long orderCode, int amount, string description,
            string returnUrl, string cancelUrl, List<PayOSItemDto> items)
        {
            var sigData = BuildSignatureString(orderCode, amount, description, cancelUrl, returnUrl);

            var body = new PayOSCreateRequest
            {
                orderCode = orderCode,
                amount = amount,    
                description = description,
                returnUrl = returnUrl,
                cancelUrl = cancelUrl,
                signature = SignPayload(sigData),
                items = items
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/payment-requests");
            request.Headers.Add("x-client-id", _clientId);
            request.Headers.Add("x-api-key", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PayOSCreateResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new PayOSCreateResponse { code = "error", desc = "Null response" };
        }

        public async Task<PayOSInfoResponse> GetPaymentInfoAsync(long orderCode)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/v2/payment-requests/{orderCode}");
            request.Headers.Add("x-client-id", _clientId);
            request.Headers.Add("x-api-key", _apiKey);

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PayOSInfoResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new PayOSInfoResponse { code = "error" };
        }

        public async Task CancelPaymentAsync(long orderCode, string reason)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/v2/payment-requests/{orderCode}");
            request.Headers.Add("x-client-id", _clientId);
            request.Headers.Add("x-api-key", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { cancellationReason = reason }),
                Encoding.UTF8, "application/json");

            await _http.SendAsync(request);
        }
    }
}
