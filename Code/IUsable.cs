using Sandbox;

public interface IUsable
{
    void OnUse( GameObject user );
    
    // (Optional) We can use this later to show UI text like "Press E to open Door"
    string GetUseText(); 
}

