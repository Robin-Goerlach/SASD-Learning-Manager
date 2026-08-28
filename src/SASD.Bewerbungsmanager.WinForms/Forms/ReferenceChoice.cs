namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Small display/value pair used by modal editors for optional entity references.</summary>
internal sealed record ReferenceChoice(Guid? Id, string Text);
