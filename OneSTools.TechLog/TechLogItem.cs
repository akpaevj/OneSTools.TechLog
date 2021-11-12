using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OneSTools.TechLog
{
    public class TechLogItem {
        /// <summary>
        /// Collection of key/value pairs
        /// </summary>
        public Dictionary<string, string> AllProperties { get; internal set; } = new Dictionary<string, string>();

        /// <summary>
        /// Date and time of the event
        /// </summary>
        public DateTime DateTime { get; internal set; }

        public long StartTicks { get; internal set; }

        public long EndTicks { get; internal set; }

        /// <summary>
        /// The duration of the event (in microseconds)
        /// </summary>
        public long Duration { get; internal set; }

        /// <summary>
        /// The end position of the event in the file
        /// </summary>
        public long EndPosition { get; internal set; }

        /// <summary>
        /// The name of the event
        /// </summary>
        public string EventName { get; internal set; }

        /// <summary>
        /// The name of the log file
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// The name of the log file folder
        /// </summary>
        public string FolderName { get; internal set; }

        /// <summary>
        /// The level of the event in the current thread's stack
        /// </summary>
        public int Level { get; internal set; }

        /// <summary>
        /// A number of the thread
        /// </summary>
        public string OSThread => AllProperties.GetValueOrDefault("OSThread", null);

        /// <summary>
        /// A name of the process
        /// </summary>
        public string Process => AllProperties.GetValueOrDefault("process", null);

        /// <summary>
        /// Время зависания процесса
        /// </summary>
        public string AbandonedTimestamp => AllProperties.GetValueOrDefault("abandonedTimestamp", null);

        /// <summary>
        /// Текстовое описание выполняемой операции во время загрузки конфигурации из файлов
        /// </summary>
        public string Action => AllProperties.GetValueOrDefault("Action", null);

        /// <summary>
        /// Имя администратора кластера или центрального сервера
        /// </summary>
        public string Admin => AllProperties.GetValueOrDefault("Admin", null);

        /// <summary>
        /// Имя администратора
        /// </summary>
        public string Administrator => AllProperties.GetValueOrDefault("Administrator", null);

        /// <summary>
        /// Адрес текущего процесса агента сервера системы «1С:Предприятие»
        /// </summary>
        public string AgentURL => AllProperties.GetValueOrDefault("agentURL", null);

        public string AlreadyLocked => AllProperties.GetValueOrDefault("AlreadyLocked", null);

        public string AppID => AllProperties.GetValueOrDefault("AppID", null);

        public string Appl => AllProperties.GetValueOrDefault("Appl", null);

        /// <summary>
        /// Уточнение требования назначения функциональности
        /// </summary>
        public string ApplicationExt => AllProperties.GetValueOrDefault("ApplicationExt", null);

        /// <summary>
        /// Количество попыток установки соединения с процессом, завершившихся ошибкой
        /// </summary>
        public long? Attempts => GetLongOrNull("Attempts");

        /// <summary>
        /// Среднее количество исключений за последние 5 минут по другим процессам
        /// </summary>
        public long? AvgExceptions => GetLongOrNull("AvgExceptions");

        /// <summary>
        /// Значение показателя Доступная память в момент вывода в технологический журнал
        /// </summary>
        public long? AvMem => GetLongOrNull("AvMem");

        /// <summary>
        /// Формирование индекса полнотекстового поиска выполнялось в фоновом процессе
        /// </summary>
        public string BackgroundJobCreated => AllProperties.GetValueOrDefault("BackgroundJobCreated", null);

        /// <summary>
        /// Размер в байтах тела запроса/ответа
        /// </summary>
        public long? Body => GetLongOrNull("Body");

        public int? CallID => GetIntOrNull("CallID");

        /// <summary>
        /// Количество обращений клиентского приложения к серверному приложению через TCP
        /// </summary>
        public int? Calls => GetIntOrNull("Calls");

        /// <summary>
        /// Описание проверяемого сертификата
        /// </summary>
        public string Certificate => AllProperties.GetValueOrDefault("certificate", null);

        /// <summary>
        /// Имя класса, в котором сгенерировано событие
        /// </summary>
        public string Class => AllProperties.GetValueOrDefault("Class", null);

        public int? CallWait => GetIntOrNull("callWait");

        /// <summary>
        /// Очищенный текст SQL запроса
        /// </summary>
        public string CleanSql => GetCleanSql(Sql);

        public string ClientComputerName => AllProperties.GetValueOrDefault("ClientComputerName", null);

        public int? ClientID => GetIntOrNull("ClientID");

        /// <summary>
        /// Номер основного порта кластера серверов
        /// </summary>
        public string Cluster => AllProperties.GetValueOrDefault("Cluster", null);

        public string ClusterID => AllProperties.GetValueOrDefault("ClusterID", null);

        public string ConnectionID => AllProperties.GetValueOrDefault("connectionID", null);

        /// <summary>
        /// Имя компоненты платформы, в которой сгенерировано событие
        /// </summary>
        public string Component => AllProperties.GetValueOrDefault("Component", null);

        /// <summary>
        /// Номер соединения с информационной базой
        /// </summary>
        public long? Connection => GetLongOrNull("Connection");

        /// <summary>
        /// Количество соединений, которым не хватило рабочих процессов
        /// </summary>
        public long? Connections => GetLongOrNull("Connections");

        /// <summary>
        /// Установленное максимальное количество соединений на один рабочий процесс
        /// </summary>
        public long? ConnLimit => GetLongOrNull("ConnLimit");

        /// <summary>
        /// Контекст исполнения
        /// </summary>
        public string Context
        {
            get => AllProperties.GetValueOrDefault("Context", null);
            set => AllProperties["Context"] = value;
        }

        /// <summary>
        /// Общий размер скопированных значений при сборке мусора
        /// </summary>
        public long? CopyBytes => GetLongOrNull("CopyBytes");

        /// <summary>
        /// Длительность вызова в микросекундах
        /// </summary>
        public long? CpuTime => GetLongOrNull("CpuTime");

        public int? CreateDump => GetIntOrNull("createDump");

        public string Cycles => AllProperties.GetValueOrDefault("Cycles", null);

        /// <summary>
        /// Количество исключений в процессе за последние 5 минут
        /// </summary>
        public long? CurExceptions => GetLongOrNull("CurExceptions");

        /// <summary>
        /// Путь к используемой базе данных
        /// </summary>
        public string Database => AllProperties.GetValueOrDefault("DataBase", null);

        /// <summary>
        /// Идентификатор соединения с СУБД внешнего источника данных
        /// </summary>
        public string DBConnID => AllProperties.GetValueOrDefault("DBConnID", null);

        /// <summary>
        /// Строка соединения с внешним источником данных
        /// </summary>
        public string DBConnStr => AllProperties.GetValueOrDefault("DBConnStr", null);

        /// <summary>
        /// Имя используемой копии базы данных
        /// </summary>
        public string DBCopy => AllProperties.GetValueOrDefault("DBCopy", null);

        /// <summary>
        /// Имя СУБД, используемой для выполнения операции, которая повлекла формирование данного события технологического журнала
        /// </summary>
        public string DBMS => AllProperties.GetValueOrDefault("DBMS", null);

        /// <summary>
        /// Строковое представление идентификатора соединения сервера системы «1С:Предприятие» с сервером баз данных в терминах сервера баз данных
        /// </summary>
        public string Dbpid => AllProperties.GetValueOrDefault("dbpid", null);

        /// <summary>
        /// Имя пользователя СУБД внешнего источника данных
        /// </summary>
        public string DBUsr => AllProperties.GetValueOrDefault("DBUsr", null);

        /// <summary>
        /// Список пар транзакций, образующих взаимную блокировку
        /// </summary>
        public string DeadlockConnectionIntersections => AllProperties.GetValueOrDefault("DeadlockConnectionIntersections", null);

        public DeadlockConnectionIntersectionsInfo DeadlockConnectionIntersectionsInfo 
            => string.IsNullOrEmpty(DeadlockConnectionIntersections) ? null : new DeadlockConnectionIntersectionsInfo(DeadlockConnectionIntersections);

        /// <summary>
        /// Пояснения к программному исключению
        /// </summary>
        public string Descr => AllProperties.GetValueOrDefault("Descr", null);

        /// <summary>
        /// Текст, поясняющий выполняемое действие
        /// </summary>
        public string Description => AllProperties.GetValueOrDefault("description", null);

        /// <summary>
        /// Назначенный адрес рабочего процесса
        /// </summary>
        public string DstAddr => AllProperties.GetValueOrDefault("DstAddr", null);

        /// <summary>
        /// Уникальный идентификатор назначенного рабочего процесса
        /// </summary>
        public string DstId => AllProperties.GetValueOrDefault("DstId", null);

        /// <summary>
        /// Системный идентификатор назначенного рабочего процесса
        /// </summary>
        public string DstPid => AllProperties.GetValueOrDefault("DstPid", null);

        /// <summary>
        /// Назначенное имя рабочего сервера
        /// </summary>
        public string DstSrv => AllProperties.GetValueOrDefault("DstSrv", null);

        public string DstClientID => AllProperties.GetValueOrDefault("DstClientID", null);

        public int? Err => GetIntOrNull("Err");

        public string Exception => AllProperties.GetValueOrDefault("Exception", null);

        public int? ExpirationTimeout => GetIntOrNull("expirationTimeout");

        public int? FailedJobsCount => GetIntOrNull("FailedJobsCount");

        public string Finish => AllProperties.GetValueOrDefault("Finish", null);

        public string First => AllProperties.GetValueOrDefault("first", null);

        /// <summary>
        /// Первая строка контекста исполнения
        /// </summary>
        public string FirstContextLine
        {
            get
            {
                if (string.IsNullOrEmpty(Context))
                    return "";

                var index = Context.IndexOf('\n');

                return index > 0 ? Context[..index].Trim() : Context;
            }
        }

        public string Func => AllProperties.GetValueOrDefault("Func", null);

        public string Headers => AllProperties.GetValueOrDefault("Headers", null);

        public string Host => AllProperties.GetValueOrDefault("Host", null);

        public string HResultNC2012 => AllProperties.GetValueOrDefault("hResultNC2012", null);

        public string Ib => AllProperties.GetValueOrDefault("IB", null);

        public string ID => AllProperties.GetValueOrDefault("ID", null);

        /// <summary>
        /// Имя передаваемого интерфейса, метод которого вызывается удаленно
        /// </summary>
        public string IName => AllProperties.GetValueOrDefault("IName", null);

        /// <summary>
        /// Количество данных, прочитанных с диска за время вызова (в байтах)
        /// </summary>
        public long? InBytes => GetLongOrNull("InBytes");

        public string InfoBaseID => AllProperties.GetValueOrDefault("InfoBaseID", null);

        public string Interface => AllProperties.GetValueOrDefault("Interface", null);

        /// <summary>
        /// Последняя строка контекста исполнения
        /// </summary>
        public string LastContextLine
        {
            get
            {
                if (string.IsNullOrEmpty(Context))
                    return "";
                else
                {
                    var index = Context.LastIndexOf('\t');

                    if (index > 0)
                        return Context[(index + 1)..].Trim();
                    else
                        return Context;
                }
            }
        }

        /// <summary>
        /// Поток является источником блокировки
        /// </summary>
        public string Lka => AllProperties.GetValueOrDefault("lka", null);

        /// <summary>
        /// Поток является жертвой блокировки
        /// </summary>
        public string Lkp => AllProperties.GetValueOrDefault("lkp", null);

        /// <summary>
        /// Номер запроса к СУБД, «кто кого заблокировал» (только для потока-жертвы блокировки). Например, ‘423’
        /// </summary>
        public string Lkpid => AllProperties.GetValueOrDefault("lkpid", null);

        /// <summary>
        /// Cписок номеров запросов к СУБД, «кто кого заблокировал» (только для потока-источника блокировки). Например, ‘271,273,274’
        /// </summary>
        public string Lkaid => AllProperties.GetValueOrDefault("lkaid", null);

        /// <summary>
        /// Массив значений из свойства lkaid
        /// </summary>
        public int[] LkaidArray => Lkaid != "" ? Lkaid.Split(',').Select(int.Parse).ToArray() : default;

        /// <summary>
        /// Номер соединения источника блокировки, если поток является жертвой, например, ‘23’
        /// </summary>
        public string Lksrc => AllProperties.GetValueOrDefault("lksrc", null);

        /// <summary>
        /// Время в секундах, прошедшее с момента обнаружения, что поток является жертвой. Например: ‘15’
        /// </summary>
        public string Lkpto => AllProperties.GetValueOrDefault("lkpto", null);

        /// <summary>
        /// Время в секундах, прошедшее с момента обнаружения, что поток является источником блокировок. Например, ‘21’
        /// </summary>
        public string Lkato => AllProperties.GetValueOrDefault("lkato", null);

        public string Locks => AllProperties.GetValueOrDefault("Locks", null);

        public LocksInfo LocksInfo => string.IsNullOrEmpty(Locks) ? null : new LocksInfo(Locks);

        public int? LogOnly => GetIntOrNull("logOnly");

        public long? Memory => GetLongOrNull("Memory");

        public long? MemoryPeak => GetLongOrNull("MemoryPeak");

        public string Method => AllProperties.GetValueOrDefault("Method", null);

        public int? MinDataId => GetIntOrNull("MinDataId");

        /// <summary>
        /// Имя удаленно вызываемого метода
        /// </summary>
        public string MName => AllProperties.GetValueOrDefault("MName", null);

        public string Module => AllProperties.GetValueOrDefault("Module", null);

        public string ModuleName => AllProperties.GetValueOrDefault("ModuleName", null);

        public string Name => AllProperties.GetValueOrDefault("Name", null);

        public int? Nmb => GetIntOrNull("Nmb");

        /// <summary>
        /// Количество данных, записанных на диск за время вызова (в байтах)
        /// </summary>
        public long? OutBytes => GetLongOrNull("OutBytes");

        public string Phrase => AllProperties.GetValueOrDefault("Phrase", null);

        public int? Pid => GetIntOrNull("Pid");

        /// <summary>
        /// План запроса, содержащегося в свойстве Sql
        /// </summary>
        public string PlanSQLText => AllProperties.GetValueOrDefault("planSQLText", null);

        /// <summary>
        /// Номер основного сетевого порта процесса
        /// </summary>
        public int? Port => GetIntOrNull("Port");

        /// <summary>
        /// Имя серверного контекста, который обычно совпадает с именем информационной базы
        /// </summary>
        public string PProcessName => AllProperties.GetValueOrDefault("p:processName", null);

        public string Prm => AllProperties.GetValueOrDefault("Prm", null);

        public string ProcedureName => AllProperties.GetValueOrDefault("ProcedureName", null);

        public string ProcessID => AllProperties.GetValueOrDefault("ProcessID", null);

        /// <summary>
        /// Наименование процесса
        /// </summary>
        public string ProcessName => AllProperties.GetValueOrDefault("ProcessName", null);

        /// <summary>
        /// Адрес процесса сервера системы «1С:Предприятие», к которому относится событие
        /// </summary>
        public string ProcURL => AllProperties.GetValueOrDefault("procURL", null);

        public int? Protected => GetIntOrNull("Protected");

        /// <summary>
        /// Текст запроса на встроенном языке, при выполнении которого обнаружилось значение NULL в поле, для которого такое значение недопустимо
        /// </summary>
        public string Query => AllProperties.GetValueOrDefault("Query", null);

        /// <summary>
        /// Перечень полей запроса, в которых обнаружены значения NULL
        /// </summary>
        public string QueryFields => AllProperties.GetValueOrDefault("QueryFields", null);

        public string Ranges => AllProperties.GetValueOrDefault("Ranges", null);

        /// <summary>
        /// Причина недоступности рабочего процесса
        /// </summary>
        public string Reason => AllProperties.GetValueOrDefault("Reason", null);

        /// <summary>
        /// Имена пространств управляемых транзакционных блокировок
        /// </summary>
        public string Regions => AllProperties.GetValueOrDefault("Regions", null);

        public string Res => AllProperties.GetValueOrDefault("res", null);

        public string Result => AllProperties.GetValueOrDefault("Result", null);

        public string RetExcp => AllProperties.GetValueOrDefault("RetExcp", null);

        /// <summary>
        /// Количество полученных записей базы данных
        /// </summary>
        public int? Rows => GetIntOrNull("Rows");

        /// <summary>
        /// Количество измененных записей базы данных
        /// </summary>
        public int? RowsAffected => GetIntOrNull("RowsAffected");

        /// <summary>
        /// Режим запуска процесса (приложение или сервис)
        /// </summary>
        public string RunAs => AllProperties.GetValueOrDefault("RunAs", null);

        public long? SafeCallMemoryLimit => GetLongOrNull("SafeCallMemoryLimit");

        /// <summary>
        /// Текст запроса на встроенном языке модели базы данных
        /// </summary>
        public string Sdbl => AllProperties.GetValueOrDefault("Sdbl", null);

        /// <summary>
        /// Имя рабочего сервера
        /// </summary>
        public string ServerComputerName => AllProperties.GetValueOrDefault("ServerComputerName", null);

        public string ServerID => AllProperties.GetValueOrDefault("ServerID", null);

        public string ServerName => AllProperties.GetValueOrDefault("ServerName", null);

        /// <summary>
        /// Номер сеанса, назначенный текущему потоку. Если текущему потоку не назначен никакой сеанс, то свойство не добавляется
        /// </summary>
        public string SessionID => AllProperties.GetValueOrDefault("SessionID", null);

        /// <summary>
        /// Текст оператора SQL
        /// </summary>
        public string Sql => AllProperties.GetValueOrDefault("Sql", null);

        /// <summary>
        /// MD5 хеш значения свойства CleanSql
        /// </summary>
        public string SqlHash => GetMd5Hash(CleanSql);

        public string SrcName => AllProperties.GetValueOrDefault("SrcName", null);

        public string SrcProcessName => AllProperties.GetValueOrDefault("SrcProcessName", null);

        public string State => AllProperties.GetValueOrDefault("State", null);

        /// <summary>
        /// Код состояния HTTP
        /// </summary>
        public int? Status => GetIntOrNull("Status");

        public int? SyncPort => GetIntOrNull("SyncPort");

        /// <summary>
        /// Объем занятой процессом динамической памяти на момент вывода события MEM (в байтах)
        /// </summary>
        public long? Sz => GetLongOrNull("Sz");

        /// <summary>
        /// Изменение объема динамической памяти, занятой процессом, с момента вывода предыдущего события MEM (в байтах)
        /// </summary>
        public long? Szd => GetLongOrNull("Szd");

        public string TableName => AllProperties.GetValueOrDefault("tableName", null);

        /// <summary>
        /// Идентификатор клиентской программы
        /// </summary>
        public string TApplicationName => AllProperties.GetValueOrDefault("t:applicationName", null);

        /// <summary>
        /// Идентификатор соединения с клиентом по TCP
        /// </summary>
        public int? TClientID => GetIntOrNull("t:clientID");

        /// <summary>
        /// Имя клиентского компьютера
        /// </summary>
        public string TComputerName => AllProperties.GetValueOrDefault("t:computerName", null);

        /// <summary>
        /// Идентификатор соединения с информационной базой
        /// </summary>
        public int? TConnectID => GetIntOrNull("t:connectID");

        /// <summary>
        /// Время вывода записи в технологический журнал
        /// **Для события ATTN содержит имя серверного процесса: rmngr или rphost
        /// </summary>
        public string Time => AllProperties.GetValueOrDefault("Time", null);

        public int? TotalJobsCount => GetIntOrNull("TotalJobsCount");
        /// <summary>
        /// Идентификатор активности транзакции на момент начала события:
        /// 0 ‑ транзакция не была открыта;
        /// 1 ‑ транзакция была открыта.
        /// </summary>
        public int? Trans => GetIntOrNull("Trans");

        public int? TTmpConnectID => GetIntOrNull("t:tmpConnectID");

        /// <summary>
        /// Текст информационного сообщения
        /// </summary>
        public string Txt 
        {
            get 
            {
                if (AllProperties.TryGetValue("Txt", out var txt))
                    return txt;
                else
                    return AllProperties.GetValueOrDefault("txt", null);
            }
        }

        /// <summary>
        /// Ресурс, к которому производится обращение
        /// </summary>
        public string URI => AllProperties.GetValueOrDefault("URI", null);

        public string UserName => AllProperties.GetValueOrDefault("UserName", null);

        /// <summary>
        /// Размер используемого места в хранилище, в байтах
        /// </summary>
        public long? UsedSize => GetLongOrNull("UsedSize");

        /// <summary>
        /// Имя пользователя информационной базы (если в информационной базе не определены пользователи, это свойство будет иметь значение DefUser). 
        /// Значение свойства берется из назначенного сеанса.
        /// </summary>
        public string Usr => AllProperties.GetValueOrDefault("Usr", null);

        /// <summary>
        /// Список соединений, с которыми идет столкновение по управляемым транзакционным блокировкам
        /// </summary>
        public string WaitConnections => AllProperties.GetValueOrDefault("WaitConnections", null);

        private long? GetLongOrNull(string propertyName)
        {
            if (AllProperties.TryGetValue(propertyName, out var value))
                return long.Parse(value);
            else
                return null;
        }

        private int? GetIntOrNull(string propertyName)
        {
            if (AllProperties.TryGetValue(propertyName, out var value))
                return int.Parse(value);
            else
                return null;
        }

        public static string GetMd5Hash(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            using var cp = MD5.Create();
            var src = Encoding.UTF8.GetBytes(str);
            var res = cp.ComputeHash(src);

            return BitConverter.ToString(res).Replace("-", null);
        }

        private string GetCleanSql(string data)
        {
            if (string.IsNullOrEmpty(data))
                return "";
            else
            {
                // Remove parameters
                int startIndex = data.IndexOf("sp_executesql", StringComparison.OrdinalIgnoreCase);

                if (startIndex < 0)
                    startIndex = 0;
                else
                    startIndex += 16;

                int e1 = data.IndexOf("', N'@P", StringComparison.OrdinalIgnoreCase);
                if (e1 < 0)
                    e1 = data.Length;

                var e2 = data.IndexOf("p_0:", StringComparison.OrdinalIgnoreCase);
                if (e2 < 0)
                    e2 = data.Length;

                var endIndex = Math.Min(e1, e2);

                // Remove temp table names, parameters and guids
                var result = Regex.Replace(data[startIndex..endIndex], @"(#tt\d+|@P\d+|\d{8}-\d{4}-\d{4}-\d{4}-\d{12})", "{RD}", RegexOptions.ExplicitCapture);

                return result;
            }
        }

        public override string ToString()
        {
            return EventName;
        }
    }
}