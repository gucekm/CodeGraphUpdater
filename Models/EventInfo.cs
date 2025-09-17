public class EventInfo
{
	public string Name { get; set; }       // Event name
	public string Type { get; set; }       // Event delegate type (e.g., EventHandler, Action<string>)
	public string Summary { get; set; }    // XML doc comment summary (if available)
    public string SourceCode { get; set; } // Full source code of the event
    public float[] Embedding { get; set; }
}
