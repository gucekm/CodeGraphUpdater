using System.Collections.Generic;
using System.Reflection;

public class ClassInfo
{
	public string Name { get; set; }                         // Class name
	public string Summary { get; set; }                      // XML doc comment summary
	public string FilePath { get; set; }                     // Source file path
	public string BaseType { get; set; }                     // Inherited base class name (if any)
	public string SourceCode { get; set; }                   // Full source code of the class

    public List<MethodInfo> Methods { get; set; } = new();   // Methods declared in the class
	public List<PropertyInfo> Properties { get; set; } = new(); // Properties declared in the class
	public List<EventInfo> Events { get; set; } = new();     // Events declared in the class
	public List<FieldInfo> Fields { get; set; } = new();     // Fields declared in the class
	public List<ConstructorInfo> Constructors { get; set; } = new(); // Constructors declared in the class

    public float[] Embedding { get; set; }
}
