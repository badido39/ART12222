namespace ART122.Data
{
    public class DTOs
    {

        public class RedevableCreateDto
        {
            public int BP { get; set; }   // ✅ ADD THIS

            public string FullName { get; set; } = "";
            public string FilsDe { get; set; } = "";
            public string Adresse { get; set; } = "";
            public int Article { get; set; }
            public string Telephone { get; set; } = "";
            public string Etablissement { get; set; } = "";
            public int NIF { get; set; }
            public string Email { get; set; } = "";
            public string Activite { get; set; } = "";

            public List<ImpotCreateDto> Impots { get; set; } = new();
        }

        public class ImpotCreateDto
        {
            public string RoleNumber { get; set; } = "";
            public int YearImpot { get; set; }   // ✅ CHANGE (better than DateTime)
            public decimal Droit { get; set; }   // ✅ NEW

            public decimal PA { get; set; }
            public decimal PR { get; set; }

            public int NatureImpotId { get; set; }
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; } = "";
        }

        public class RedevableTaxSummaryDto
        {
            public int RedevableId { get; set; }
            public int BP { get; set; }
            public string FullName { get; set; } = "";

            public decimal TotalPA { get; set; }
            public decimal TotalPR { get; set; }
            public decimal TotalDroit { get; set; }
        }
    }
}
