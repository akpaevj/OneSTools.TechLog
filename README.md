# OneSTools.TechLog
[![Nuget](https://img.shields.io/nuget/v/OneSTools.TechLog)](https://www.nuget.org/packages/OneSTools.TechLog)<br>
Библиотека позволяет выполнять парсинг технологического журнала и получать каждое событие (Event) в нормализованном виде (Dictionary<string, string>), где Key - это имя свойства, а Value - это значение свойства. Реализована на основе TPL от Microsoft (Dataflow) и использует все преимущества конвейерной обработки данных.

Пример использования:
```csharp
private async Task ReadTL()
{
  var parser = new TechLogParser(@"C:\TechLogData", EventHandler);
  await parser.Parse();
}

private void EventHandler(Dictionary<string, string> eventData)
{
  switch (eventData["EventName"])
  {
    case "DBMSSQL":
      var sql = eventData["Sql"];
      var user = eventData["Usr"];
      // To do something
      break;
    default:
      break;
  }
}
```
