namespace ART122.Data
{
    public class Impot
    {
        public int Id { get; set; }
        public int BP { get; set; }
        public string RoleNumber { get; set; } = string.Empty;
        public int YearImpot { get; set; }   // ✅ CHANGE

        public decimal Droit { get; set; }

        public decimal PA { get; set; }
        public decimal PR { get; set; }

        // FK → Redevable
        public int RedevableInfoId { get; set; }
        public RedevableInfo RedevableInfo { get; set; } = null!;

        // SINGLE nature (not list)
        public int NatureImpotId { get; set; }
        public NatureImpot NatureImpot { get; set; } = null!;
        //NEW 
    }


    public class NatureImpot
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Ex TVA , TAP , TAIC , IBS ....etc
    }


    public class RedevableInfo
    {
        public int Id { get; set; }
        public int BP { get; set; }
        public string FullName { get; set; } = string.Empty; // Maybe Raison Sociale for company
        public string FilsDe { get; set; } = string.Empty; // For individuals
        public string Adresse { get; set; } = string.Empty;
        public int Article { get; set; }
        public string Telephone { get; set; } = string.Empty;
        public string Etablissement { get; set; } = string.Empty;
        public int NIF { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Activite { get; set; } = string.Empty;
        //new 
        public string? NumExtraitDeRole { get; set; }
        public DateTime DateExtraitDeRole { get; set; }


        //new Date declartation pour Saisis les Ancien Decalaration
        public DateTime DateDeclaration { get; set; } = DateTime.Now;

        // Navigation
        public List<Impot> Impots { get; set; } = new();

    }
    public class Declaration
    {
        public int Id { get; set; }
        public int Number { get; set; }   // auto-incremented chrono number
        public DateTime Date { get; set; } = DateTime.Today;

        // FK
        public int RedevableInfoId { get; set; }
        public RedevableInfo RedevableInfo { get; set; } = null!;

        // Payment method
        public bool DafaaWahida { get; set; }   // true = دفعة واحدة
        public bool AlaAqsat { get; set; }   // true = على أقساط

        // Amounts (snapshot at declaration time)
        public decimal Droit { get; set; }   // الحقوق البسيطة
        public decimal PA { get; set; }   // غرامات الوعاء
        public decimal PR { get; set; }   // غرامات التحصيل
        public decimal MontantTotal { get; set; }   // الإجمالي
        public decimal MontantRestant { get; set; }   // Droit × 70%
        public List<Versement> Versements { get; set; } = new();
        public DateTime? Annex6SentAt { get; set; }  // null = pending, set = already sent
        public int? Annex6BatchNumber { get; set; }  // 1, 2, 3 ... assigned at send time
        public string? NumeroQuittance { get; set; }
        public DateTime? DatePaiement { get; set; }

        // ── DG Decision ────────────────────────────────────────────────────────────
        public DGDecisionStatus DGDecision { get; set; } = DGDecisionStatus.EnAttente;
        public DateTime? DGDecisionDate { get; set; }
        public string? DGDecisionReference { get; set; }  // e.g. رقم القرار / N° décision
        public string? DGDecisionNote { get; set; }        // motif de refus or remarks

        // ── Annex 9 — ألغاء الغرامات (set when DG accepts) ────────────────────────
        // DG may cancel all or part of PA/PR — editable at decision time
        public decimal AlghaPA { get; set; }   // مبلغ إلغاء غرامات الوعاء
        public decimal AlghaPR { get; set; }   // مبلغ إلغاء غرامات التحصيل
        public decimal AlghaTotal => AlghaPA + AlghaPR;  // computed, no column needed

        // ── Annex 5 flag (accepted = appears validated in register) ────────────────
        // Derived: DGDecision == Acceptee, no extra column needed
        public bool IsValidatedByDG => DGDecision == DGDecisionStatus.Acceptee;
    }


    public enum DGDecisionStatus
    {
        EnAttente,  // awaiting DG verdict
        Acceptee,   // accepted → triggers annex 5 update + annex 9 calculation
        Refusee     // refused → declaration stays but no penalty cancellation
    }
    public class Versement
    {
        public int Id { get; set; }
        public int NumeroOrdre { get; set; }
        public DateTime DatePrevue { get; set; }
        public DateTime? DatePaiement { get; set; }
        public decimal Montant { get; set; }
        public string NumeroQuittance { get; set; } = string.Empty;
        public VersementStatus Status { get; set; }
        public bool EstPaye { get; set; }

        public int DeclarationId { get; set; }
        public Declaration Declaration { get; set; } = null!;
        
    }
    public enum VersementStatus
    {
        EnAttente,
        Paye,
        EnRetard,
        Annule
    }
}
