namespace SchoolApp.DTOs
{
    public class PayOSCreateRequest
    {
        public long orderCode { get; set; }
        public int amount { get; set; }
        public string description { get; set; } = "";
        public string returnUrl { get; set; } = "";
        public string cancelUrl { get; set; } = "";
        public string signature { get; set; } = "";
        public List<PayOSItemDto> items { get; set; } = new();
    }

    public class PayOSItemDto
    {
        public string name { get; set; } = "";
        public int quantity { get; set; }
        public int price { get; set; }
    }

    public class PayOSCreateResponse
    {
        public string code { get; set; } = "";
        public string desc { get; set; } = "";
        public PayOSCreateData? data { get; set; }
    }

    public class PayOSCreateData
    {
        public string checkoutUrl { get; set; } = "";
        public string status { get; set; } = "";
    }

    public class PayOSInfoResponse
    {
        public string code { get; set; } = "";
        public PayOSInfoData? data { get; set; }
    }

    public class PayOSInfoData
    {
        public string status { get; set; } = "";
        public long orderCode { get; set; }
        public int amount { get; set; }
    }
}

