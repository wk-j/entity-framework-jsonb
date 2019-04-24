## Json B

```csharp
 public class StudentSettingsHandler : TypeHandler<StudentSettings> {
    private StudentSettingsHandler() { }
    public static StudentSettingsHandler Instance { get; } = new StudentSettingsHandler();
    public override StudentSettings Parse(object value) {
        var stringValue = (string)value;
        var json = string.IsNullOrEmpty(stringValue) ? "{}" : stringValue;
        return JObject.Parse(json).ToObject<StudentSettings>();
    }
    public override void SetValue(IDbDataParameter parameter, StudentSettings value) {
        parameter.Value = JsonConvert.SerializeObject(value);
    }
}

SqlMapper.AddTypeHandler(StudentSettingsHandler.Instance);
```