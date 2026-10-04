using Microsoft.Win32;

// The /c dialog: pick which clock skin the screensaver shows. The choice is
// stored per user in the registry and read back by ScreensaverForm, which
// passes it to the app as ?skin=.
public class SettingsForm : Form
{
    // Must match the Skins array in Pages/AnalogClock.razor - a skin added there
    // has to be added here too. The first entry is the default.
    public static readonly string[] Skins = { "field", "blob", "aurora", "lens", "glass", "classic" };

    private const string KeyPath = @"Software\GiampaLab\TimeScreensaver";
    private const string ValueName = "Skin";

    private readonly ComboBox _skin = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };

    public SettingsForm()
    {
        Text = "Time Screensaver";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _skin.Items.AddRange(Skins);
        _skin.SelectedItem = LoadSkin();

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        ok.Click += (_, _) => SaveSkin((string)_skin.SelectedItem!);
        AcceptButton = ok;
        CancelButton = cancel;

        // AutoSize panels grow to fit their buttons; Dock = Fill would instead
        // pin the panel to the row height and clip them.
        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 8, 0, 0),
        };
        buttons.Controls.AddRange(new Control[] { cancel, ok });

        var layout = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        layout.Controls.Add(new Label { Text = "Clock skin:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        layout.Controls.Add(_skin, 1, 0);
        layout.Controls.Add(buttons, 0, 1);
        layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout);
    }

    // The saved skin, or the default if nothing (or something no longer valid)
    // is stored.
    public static string LoadSkin()
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
        var saved = key?.GetValue(ValueName) as string;
        return Array.IndexOf(Skins, saved) >= 0 ? saved! : Skins[0];
    }

    private static void SaveSkin(string skin)
    {
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
        key.SetValue(ValueName, skin);
    }
}
