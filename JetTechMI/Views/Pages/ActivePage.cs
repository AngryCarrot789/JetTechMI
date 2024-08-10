namespace JetTechMI.Views.Pages;

public enum ActivePage {
    // Main page is the first just in case there's an error that defaults the active
    // page to default. We don't want to be permanently stuck on the startup page
    Main = 0,
}