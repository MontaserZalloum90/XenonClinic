namespace XenonClinic.Core.Enums;

public enum InventoryTransactionType
{
    Purchase = 0,      // Stock coming in from supplier
    Sale = 1,          // Stock going out to patient
    Adjustment = 2,    // Manual adjustment (damage, loss, found)
    Transfer = 3,      // Transfer between branches
    Return = 4         // Return from patient or to supplier
}
