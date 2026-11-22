using Sevro.Models.Enums;

namespace Sevro.Helpers
{
    public static class PricingFormatter
    {
        public static string FormatPrice(decimal? price, PricingType pricingType, string? unit = null)
        {
            if (pricingType == PricingType.QuoteRequired)
            {
                return "Get Quote";
            }

            var amount = price.HasValue ? $"₹{price.Value:N0}" : "Get Quote";
            var cleanUnit = NormalizeUnitSuffix(unit);

            return pricingType switch
            {
                PricingType.Fixed => amount,
                PricingType.StartingFrom => string.IsNullOrEmpty(cleanUnit)
                    ? $"Starting {amount}"
                    : $"Starting {amount}/{cleanUnit}",
                PricingType.Hourly => $"{amount}/hour",
                PricingType.PerVisit => $"{amount}/visit",
                PricingType.PerUnit => string.IsNullOrEmpty(cleanUnit)
                    ? amount
                    : $"{amount}/{cleanUnit}",
                _ => amount
            };
        }

        public static string GetPricingTypeLabel(PricingType pricingType) => pricingType switch
        {
            PricingType.Fixed => "Fixed Price",
            PricingType.StartingFrom => "Starting Price",
            PricingType.Hourly => "Hourly Rate",
            PricingType.PerVisit => "Visit / Inspection Fee",
            PricingType.PerUnit => "Per Unit",
            PricingType.QuoteRequired => "Quotation Required",
            _ => "Fixed Price"
        };

        public static string GetPricingExplanation(PricingType pricingType, string? unit = null) => pricingType switch
        {
            PricingType.Fixed => "Transparent fixed rate for one complete service/task. No hidden charges — pay directly on completion.",
            PricingType.StartingFrom => "Starting estimate for standard work. Final price is confirmed transparently with you before extra work begins.",
            PricingType.Hourly => "Charged transparently per hour of active work based on job duration.",
            PricingType.PerVisit => "Covers technician visit and on-site inspection. Any optional repair cost is shared clearly before starting.",
            PricingType.PerUnit => $"Transparent rate billed {(string.IsNullOrWhiteSpace(unit) ? "per unit" : unit.ToLowerInvariant())} based on exact quantity.",
            PricingType.QuoteRequired => "Complex or custom job — submit your requirements for free. Your provider will send a clear ₹ quotation for your approval before work begins.",
            _ => "Transparent local service rate."
        };

        public static string GetPricingBadgeClass(PricingType pricingType) => pricingType switch
        {
            PricingType.Fixed => "sc-status-completed",
            PricingType.StartingFrom => "sc-status-confirmed",
            PricingType.Hourly => "sc-status-inprogress",
            PricingType.PerVisit => "sc-status-pending",
            PricingType.PerUnit => "sc-status-confirmed",
            PricingType.QuoteRequired => "sc-status-pending",
            _ => "sc-status-muted"
        };

        public static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "item";
            var chars = input.Trim().ToLowerInvariant()
                .Replace("&", " and ")
                .Replace("'", "")
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray();
            var raw = new string(chars);
            while (raw.Contains("--")) raw = raw.Replace("--", "-");
            return raw.Trim('-');
        }

        private static string NormalizeUnitSuffix(string? unit)
        {
            if (string.IsNullOrWhiteSpace(unit)) return string.Empty;
            var u = unit.Trim().ToLowerInvariant();
            if (u is "per job" or "per service" or "per task" or "custom" or "fixed") return string.Empty;
            if (u.StartsWith("per ")) u = u[4..].Trim();
            return u;
        }
    }
}
