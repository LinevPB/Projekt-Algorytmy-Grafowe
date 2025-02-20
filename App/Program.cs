using System.Runtime.InteropServices;
using GraphManagementApp.UI;

class Program
{
    // Attribute required for Windows Forms applications
    [STAThread]

    // Import WinAPI function to get the console window handle
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    // Import WinAPI function to change window visibility
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // Constants for hiding and showing the console window
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    static void Main()
    {
        // Create a new thread for the UI
        Thread uiThread = new Thread(() =>
        {
            // Get the console window handle
            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, SW_HIDE); // Hide the console
            }

            // Configure Windows Forms application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WindowUI()); // Run the main application window
        });

        // Set the thread to Single Thread Apartment (STA) mode - required for Windows Forms
        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.Start();
    }
}
