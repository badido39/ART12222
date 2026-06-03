namespace ART122.Data
{
    public class DTOs
    {

        public class RedevableCreateDto
        {
            public SecteurActivite Secteur { get; set; } = SecteurActivite.Prive;

            public string BP { get; set; }   // ✅ ADD THIS

            public string FullName { get; set; } = "";
            public string FilsDe { get; set; } = "";
            public string Adresse { get; set; } = "";
            public string Article { get; set; } = string.Empty;
            public string Telephone { get; set; } = "";
            public string Etablissement { get; set; } = "";
            public string NIF { get; set; } = string.Empty; public string Email { get; set; } = "";
            public string Activite { get; set; } = "";


            //new 
            public string? NumExtraitDeRole { get; set; }
            public DateTime DateExtraitDeRole { get; set; }

            // date declaration
            public DateTime DateDeclaration { get; set; } = DateTime.Now;


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
            public string BP { get; set; }
            public string FullName { get; set; } = "";

            public decimal TotalPA { get; set; }
            public decimal TotalPR { get; set; }
            public decimal TotalDroit { get; set; }
        }
    }
}
