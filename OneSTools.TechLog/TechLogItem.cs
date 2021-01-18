using System;
using System.Collections.Generic;
using System.Linq;

namespace OneSTools.TechLog
{
    public class TechLogItem    {
        /// <summary>
        /// Collection of key/value pairs
        /// </summary>
        public Dictionary<string, string> AllProperties { get; internal set; } = new Dictionary<string, string>();

        /// <summary>
        /// Date and time of the event
        /// </summary>
        public DateTime DateTime { get; internal set; }

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
        public string OSThread => AllProperties.GetValueOrDefault("OSThread", "");

        /// <summary>
        /// A name of the process
        /// </summary>
        public string Process => AllProperties.GetValueOrDefault("process", "");

        /// <summary>
        /// Время зависания процесса
        /// </summary>
        public string AbandonedTimestamp => AllProperties.GetValueOrDefault("abandonedTimestamp", "");

        /// <summary>
        /// Текстовое описание выполняемой операции во время загрузки конфигурации из файлов
        /// </summary>
        public string Action => AllProperties.GetValueOrDefault("Action", "");

        /// <summary>
        /// Имя администратора кластера или центрального сервера
        /// </summary>
        public string Admin => AllProperties.GetValueOrDefault("Admin", "");

        /// <summary>
        /// Имя администратора
        /// </summary>
        public string Administrator => AllProperties.GetValueOrDefault("Administrator", "");

        /// <summary>
        /// Адрес текущего процесса агента сервера системы «1С:Предприятие»
        /// </summary>
        public string AgentURL => AllProperties.GetValueOrDefault("agentURL", "");

        public string AlreadyLocked => AllProperties.GetValueOrDefault("AlreadyLocked", "");

        public string AppID => AllProperties.GetValueOrDefault("AppID", "");

        public string Appl => AllProperties.GetValueOrDefault("Appl", "");

        /// <summary>
        /// Уточнение требования назначения функциональности
        /// </summary>
        public string ApplicationExt => AllProperties.GetValueOrDefault("ApplicationExt", "");

        /// <summary>
        /// Количество попыток установки соединения с процессом, завершившихся ошибкой
        /// </summary>
        public long Attempts => long.Parse(AllProperties.GetValueOrDefault("Attempts", "-1"));

        /// <summary>
        /// Среднее количество исключений за последние 5 минут по другим процессам
        /// </summary>
        public long AvgExceptions => long.Parse(AllProperties.GetValueOrDefault("AvgExceptions", "-1"));

        /// <summary>
        /// Значение показателя Доступная память в момент вывода в технологический журнал
        /// </summary>
        public long AvMem => long.Parse(AllProperties.GetValueOrDefault("AvMem", "-1"));

        /// <summary>
        /// Формирование индекса полнотекстового поиска выполнялось в фоновом процессе
        /// </summary>
        public string BackgroundJobCreated => AllProperties.GetValueOrDefault("BackgroundJobCreated", "");

        /// <summary>
        /// Размер в байтах тела запроса/ответа
        /// </summary>
        public long Body => long.Parse(AllProperties.GetValueOrDefault("Body", "-1"));

        public int CallID => int.Parse(AllProperties.GetValueOrDefault("CallID", "-1"));

        /// <summary>
        /// Количество обращений клиентского приложения к серверному приложению через TCP
        /// </summary>
        public int Calls => int.Parse(AllProperties.GetValueOrDefault("Calls", "-1"));

        /// <summary>
        /// Описание проверяемого сертификата
        /// </summary>
        public string Certificate => AllProperties.GetValueOrDefault("certificate", "");

        /// <summary>
        /// Имя класса, в котором сгенерировано событие
        /// </summary>
        public string Class => AllProperties.GetValueOrDefault("Class", "");

        public int CallWait => int.Parse(AllProperties.GetValueOrDefault("callWait", "-1"));

        /// <summary>
        /// Очищенный текст SQL запроса
        /// </summary>
        public string CleanSql => AllProperties.GetValueOrDefault("CleanSql", "");

        public string ClientComputerName => AllProperties.GetValueOrDefault("ClientComputerName", "");

        public int ClientID => int.Parse(AllProperties.GetValueOrDefault("ClientID", "-1"));

        /// <summary>
        /// Номер основного порта кластера серверов
        /// </summary>
        public string Cluster => AllProperties.GetValueOrDefault("Cluster", "");

        public string ClusterID => AllProperties.GetValueOrDefault("ClusterID", "");

        public string ConnectionID => AllProperties.GetValueOrDefault("connectionID", "");

        /// <summary>
        /// Имя компоненты платформы, в которой сгенерировано событие
        /// </summary>
        public string Component => AllProperties.GetValueOrDefault("Component", "");

        /// <summary>
        /// Номер соединения с информационной базой
        /// </summary>
        public long Connection => long.Parse(AllProperties.GetValueOrDefault("Connection", "-1"));

        /// <summary>
        /// Количество соединений, которым не хватило рабочих процессов
        /// </summary>
        public long Connections => long.Parse(AllProperties.GetValueOrDefault("Connections", "-1"));

        /// <summary>
        /// Установленное максимальное количество соединений на один рабочий процесс
        /// </summary>
        public long ConnLimit => long.Parse(AllProperties.GetValueOrDefault("ConnLimit", "-1"));

        /// <summary>
        /// Контекст исполнения
        /// </summary>
        public string Context => AllProperties.GetValueOrDefault("Context", "");

        /// <summary>
        /// Общий размер скопированных значений при сборке мусора
        /// </summary>
        public long CopyBytes => long.Parse(AllProperties.GetValueOrDefault("CopyBytes", "-1"));

        /// <summary>
        /// Длительность вызова в микросекундах
        /// </summary>
        public long CpuTime => long.Parse(AllProperties.GetValueOrDefault("CpuTime", "-1"));

        public int CreateDump => int.Parse(AllProperties.GetValueOrDefault("createDump", "-1"));

        public string Cycles => AllProperties.GetValueOrDefault("Cycles", "");

        /// <summary>
        /// Количество исключений в процессе за последние 5 минут
        /// </summary>
        public long CurExceptions => long.Parse(AllProperties.GetValueOrDefault("CurExceptions", "-1"));

        /// <summary>
        /// Путь к используемой базе данных
        /// </summary>
        public string Database => AllProperties.GetValueOrDefault("DataBase", "");

        /// <summary>
        /// Идентификатор соединения с СУБД внешнего источника данных
        /// </summary>
        public string DBConnID => AllProperties.GetValueOrDefault("DBConnID", "");

        /// <summary>
        /// Строка соединения с внешним источником данных
        /// </summary>
        public string DBConnStr => AllProperties.GetValueOrDefault("DBConnStr", "");

        /// <summary>
        /// Имя используемой копии базы данных
        /// </summary>
        public string DBCopy => AllProperties.GetValueOrDefault("DBCopy", "");

        /// <summary>
        /// Имя СУБД, используемой для выполнения операции, которая повлекла формирование данного события технологического журнала
        /// </summary>
        public string DBMS => AllProperties.GetValueOrDefault("DBMS", "");

        /// <summary>
        /// Строковое представление идентификатора соединения сервера системы «1С:Предприятие» с сервером баз данных в терминах сервера баз данных
        /// </summary>
        public string Dbpid => AllProperties.GetValueOrDefault("dbpid", "");

        /// <summary>
        /// Имя пользователя СУБД внешнего источника данных
        /// </summary>
        public string DBUsr => AllProperties.GetValueOrDefault("DBUsr", "");

        /// <summary>
        /// Список пар транзакций, образующих взаимную блокировку
        /// </summary>
        public string DeadlockConnectionIntersections => AllProperties.GetValueOrDefault("DeadlockConnectionIntersections", "");

        /// <summary>
        /// Пояснения к программному исключению
        /// </summary>
        public string Descr => AllProperties.GetValueOrDefault("Descr", "");

        /// <summary>
        /// Текст, поясняющий выполняемое действие
        /// </summary>
        public string Description => AllProperties.GetValueOrDefault("description", "");

        /// <summary>
        /// Назначенный адрес рабочего процесса
        /// </summary>
        public string DstAddr => AllProperties.GetValueOrDefault("DstAddr", "");

        /// <summary>
        /// Уникальный идентификатор назначенного рабочего процесса
        /// </summary>
        public string DstId => AllProperties.GetValueOrDefault("DstId", "");

        /// <summary>
        /// Системный идентификатор назначенного рабочего процесса
        /// </summary>
        public string DstPid => AllProperties.GetValueOrDefault("DstPid", "");

        /// <summary>
        /// Назначенное имя рабочего сервера
        /// </summary>
        public string DstSrv => AllProperties.GetValueOrDefault("DstSrv", "");

        public string DstClientID => AllProperties.GetValueOrDefault("DstClientID", "");

        public long Durationus => long.Parse(AllProperties.GetValueOrDefault("Durationus", "-1"));

        public int Err => int.Parse(AllProperties.GetValueOrDefault("Err", "-1"));

        public string Exception => AllProperties.GetValueOrDefault("Exception", "");

        public int ExpirationTimeout => int.Parse(AllProperties.GetValueOrDefault("expirationTimeout", "-1"));

        public int FailedJobsCount => int.Parse(AllProperties.GetValueOrDefault("FailedJobsCount", "-1"));

        public string Finish => AllProperties.GetValueOrDefault("Finish", "");

        public string First => AllProperties.GetValueOrDefault("first", "");

        /// <summary>
        /// Первая строка контекста исполнения
        /// </summary>
        public string FirstContextLine => AllProperties.GetValueOrDefault("FirstContextLine", "");

        public string Func => AllProperties.GetValueOrDefault("Func", "");

        public string Headers => AllProperties.GetValueOrDefault("Headers", "");

        public string Host => AllProperties.GetValueOrDefault("Host", "");

        public string HResultNC2012 => AllProperties.GetValueOrDefault("hResultNC2012", "");

        public string Ib => AllProperties.GetValueOrDefault("IB", "");

        public string ID => AllProperties.GetValueOrDefault("ID", "");

        /// <summary>
        /// Имя передаваемого интерфейса, метод которого вызывается удаленно
        /// </summary>
        public string IName => AllProperties.GetValueOrDefault("IName", "");

        /// <summary>
        /// Количество данных, прочитанных с диска за время вызова (в байтах)
        /// </summary>
        public long InBytes => long.Parse(AllProperties.GetValueOrDefault("InBytes", "-1"));

        public string InfoBaseID => AllProperties.GetValueOrDefault("InfoBaseID", "");

        public string Interface => AllProperties.GetValueOrDefault("Interface", "");

        /// <summary>
        /// Последняя строка контекста исполнения
        /// </summary>
        public string LastContextLine => AllProperties.GetValueOrDefault("LastContextLine", "");

        /// <summary>
        /// Поток является источником блокировки
        /// </summary>
        public string Lka => AllProperties.GetValueOrDefault("lka", "");

        /// <summary>
        /// Поток является жертвой блокировки
        /// </summary>
        public string Lkp => AllProperties.GetValueOrDefault("lkp", "");

        /// <summary>
        /// Номер запроса к СУБД, «кто кого заблокировал» (только для потока-жертвы блокировки). Например, ‘423’
        /// </summary>
        public string Lkpid => AllProperties.GetValueOrDefault("lkpid", "");

        /// <summary>
        /// Cписок номеров запросов к СУБД, «кто кого заблокировал» (только для потока-источника блокировки). Например, ‘271,273,274’
        /// </summary>
        public string Lkaid => AllProperties.GetValueOrDefault("lkaid", "");

        /// <summary>
        /// Массив значений из свойства lkaid
        /// </summary>
        public int[] LkaidArray => Lkaid != "" ? Lkaid.Split(',').Select(c => int.Parse(c)).ToArray() : default;

        /// <summary>
        /// Номер соединения источника блокировки, если поток является жертвой, например, ‘23’
        /// </summary>
        public string Lksrc => AllProperties.GetValueOrDefault("lksrc", "");

        /// <summary>
        /// Время в секундах, прошедшее с момента обнаружения, что поток является жертвой. Например: ‘15’
        /// </summary>
        public string Lkpto => AllProperties.GetValueOrDefault("lkpto", "");

        /// <summary>
        /// Время в секундах, прошедшее с момента обнаружения, что поток является источником блокировок. Например, ‘21’
        /// </summary>
        public string Lkato => AllProperties.GetValueOrDefault("lkato", "");

        public string Locks => AllProperties.GetValueOrDefault("Locks", "");

        public int LogOnly => int.Parse(AllProperties.GetValueOrDefault("logOnly", "-1"));

        public long Memory => long.Parse(AllProperties.GetValueOrDefault("Memory", "-1"));

        public long MemoryPeak => long.Parse(AllProperties.GetValueOrDefault("MemoryPeak", "-1"));

        public string Method => AllProperties.GetValueOrDefault("Method", "");

        public int MinDataId => int.Parse(AllProperties.GetValueOrDefault("MinDataId", "-1"));

        /// <summary>
        /// Имя удаленно вызываемого метода
        /// </summary>
        public string MName => AllProperties.GetValueOrDefault("MName", "");

        public string Module => AllProperties.GetValueOrDefault("Module", "");

        public string ModuleName => AllProperties.GetValueOrDefault("ModuleName", "");

        public string Name => AllProperties.GetValueOrDefault("Name", "");

        public int Nmb => int.Parse(AllProperties.GetValueOrDefault("Nmb", "-1"));

        /// <summary>
        /// Количество данных, записанных на диск за время вызова (в байтах)
        /// </summary>
        public long OutBytes => long.Parse(AllProperties.GetValueOrDefault("OutBytes", "-1"));

        public string Phrase => AllProperties.GetValueOrDefault("Phrase", "");

        public int Pid => int.Parse(AllProperties.GetValueOrDefault("Pid", "-1"));

        /// <summary>
        /// План запроса, содержащегося в свойстве Sql
        /// </summary>
        public string PlanSQLText => AllProperties.GetValueOrDefault("planSQLText", "");

        /// <summary>
        /// Номер основного сетевого порта процесса
        /// </summary>
        public int Port => int.Parse(AllProperties.GetValueOrDefault("Port", "-1"));

        /// <summary>
        /// Имя серверного контекста, который обычно совпадает с именем информационной базы
        /// </summary>
        public string PProcessName => AllProperties.GetValueOrDefault("p:processName", "");

        public string Prm => AllProperties.GetValueOrDefault("Prm", "");

        public string ProcedureName => AllProperties.GetValueOrDefault("ProcedureName", "");

        public string ProcessID => AllProperties.GetValueOrDefault("ProcessID", "");

        /// <summary>
        /// Наименование процесса
        /// </summary>
        public string ProcessName => AllProperties.GetValueOrDefault("ProcessName", "");

        /// <summary>
        /// Адрес процесса сервера системы «1С:Предприятие», к которому относится событие
        /// </summary>
        public string ProcURL => AllProperties.GetValueOrDefault("procURL", "");

        public int Protected => int.Parse(AllProperties.GetValueOrDefault("Protected", "-1"));

        /// <summary>
        /// Текст запроса на встроенном языке, при выполнении которого обнаружилось значение NULL в поле, для которого такое значение недопустимо
        /// </summary>
        public string Query => AllProperties.GetValueOrDefault("Query", "");

        /// <summary>
        /// Перечень полей запроса, в которых обнаружены значения NULL
        /// </summary>
        public string QueryFields => AllProperties.GetValueOrDefault("QueryFields", "");

        public string Ranges => AllProperties.GetValueOrDefault("Ranges", "");

        /// <summary>
        /// Причина недоступности рабочего процесса
        /// </summary>
        public string Reason => AllProperties.GetValueOrDefault("Reason", "");

        /// <summary>
        /// Имена пространств управляемых транзакционных блокировок
        /// </summary>
        public string Regions => AllProperties.GetValueOrDefault("Regions", "");

        public string Res => AllProperties.GetValueOrDefault("res", "");

        public string Result => AllProperties.GetValueOrDefault("Result", "");

        public string RetExcp => AllProperties.GetValueOrDefault("RetExcp", "");

        /// <summary>
        /// Количество полученных записей базы данных
        /// </summary>
        public int Rows => int.Parse(AllProperties.GetValueOrDefault("Rows", "-1"));

        /// <summary>
        /// Количество измененных записей базы данных
        /// </summary>
        public int RowsAffected => int.Parse(AllProperties.GetValueOrDefault("RowsAffected", "-1"));

        /// <summary>
        /// Режим запуска процесса (приложение или сервис)
        /// </summary>
        public string RunAs => AllProperties.GetValueOrDefault("RunAs", "");

        public long SafeCallMemoryLimit => long.Parse(AllProperties.GetValueOrDefault("SafeCallMemoryLimit", "-1"));

        /// <summary>
        /// Текст запроса на встроенном языке модели базы данных
        /// </summary>
        public string Sdbl => AllProperties.GetValueOrDefault("Sdbl", "");

        /// <summary>
        /// Имя рабочего сервера
        /// </summary>
        public string ServerComputerName => AllProperties.GetValueOrDefault("ServerComputerName", "");

        public string ServerID => AllProperties.GetValueOrDefault("ServerID", "");

        public string ServerName => AllProperties.GetValueOrDefault("ServerName", "");

        /// <summary>
        /// Номер сеанса, назначенный текущему потоку. Если текущему потоку не назначен никакой сеанс, то свойство не добавляется
        /// </summary>
        public string SessionID => AllProperties.GetValueOrDefault("SessionID", "");

        /// <summary>
        /// Текст оператора SQL
        /// </summary>
        public string Sql => AllProperties.GetValueOrDefault("Sql", "");

        /// <summary>
        /// MD5 хеш значения свойства CleanSql
        /// </summary>
        public string SqlHash => AllProperties.GetValueOrDefault("SqlHash", "");

        public string SrcName => AllProperties.GetValueOrDefault("SrcName", "");

        public string SrcProcessName => AllProperties.GetValueOrDefault("SrcProcessName", "");

        public string State => AllProperties.GetValueOrDefault("State", "");

        /// <summary>
        /// Код состояния HTTP
        /// </summary>
        public int Status => int.Parse(AllProperties.GetValueOrDefault("Status", "-1"));

        public int SyncPort => int.Parse(AllProperties.GetValueOrDefault("SyncPort", "-1"));

        /// <summary>
        /// Объем занятой процессом динамической памяти на момент вывода события MEM (в байтах)
        /// </summary>
        public long Sz => long.Parse(AllProperties.GetValueOrDefault("Sz", "-1"));

        /// <summary>
        /// Изменение объема динамической памяти, занятой процессом, с момента вывода предыдущего события MEM (в байтах)
        /// </summary>
        public long Szd => long.Parse(AllProperties.GetValueOrDefault("Szd", "-1"));

        public string TableName => AllProperties.GetValueOrDefault("tableName", "");

        /// <summary>
        /// Идентификатор клиентской программы
        /// </summary>
        public string TApplicationName => AllProperties.GetValueOrDefault("t:applicationName", "");

        /// <summary>
        /// Идентификатор соединения с клиентом по TCP
        /// </summary>
        public int TClientID => int.Parse(AllProperties.GetValueOrDefault("t:clientID", "-1"));

        /// <summary>
        /// Имя клиентского компьютера
        /// </summary>
        public string TComputerName => AllProperties.GetValueOrDefault("t:computerName", "");

        /// <summary>
        /// Идентификатор соединения с информационной базой
        /// </summary>
        public int TConnectID => int.Parse(AllProperties.GetValueOrDefault("t:connectID", "-1"));

        /// <summary>
        /// Время вывода записи в технологический журнал
        /// **Для события ATTN содержит имя серверного процесса: rmngr или rphost
        /// </summary>
        public string Time => AllProperties.GetValueOrDefault("Time", "");

        public int TotalJobsCount => int.Parse(AllProperties.GetValueOrDefault("TotalJobsCount", "-1"));
        /// <summary>
        /// Идентификатор активности транзакции на момент начала события:
        /// 0 ‑ транзакция не была открыта;
        /// 1 ‑ транзакция была открыта.
        /// </summary>
        public int Trans => int.Parse(AllProperties.GetValueOrDefault("Trans", "-1"));

        public int TTmpConnectID => int.Parse(AllProperties.GetValueOrDefault("t:tmpConnectID", "-1"));

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
                    return AllProperties.GetValueOrDefault("txt", "");
            }
        }

        /// <summary>
        /// Ресурс, к которому производится обращение
        /// </summary>
        public string URI => AllProperties.GetValueOrDefault("URI", "");

        public string UserName => AllProperties.GetValueOrDefault("UserName", "");

        /// <summary>
        /// Размер используемого места в хранилище, в байтах
        /// </summary>
        public long UsedSize => long.Parse(AllProperties.GetValueOrDefault("UsedSize", "-1"));

        /// <summary>
        /// Имя пользователя информационной базы (если в информационной базе не определены пользователи, это свойство будет иметь значение DefUser). 
        /// Значение свойства берется из назначенного сеанса.
        /// </summary>
        public string Usr => AllProperties.GetValueOrDefault("Usr", "");

        /// <summary>
        /// Список соединений, с которыми идет столкновение по управляемым транзакционным блокировкам
        /// </summary>
        public string WaitConnections => AllProperties.GetValueOrDefault("WaitConnections", "");
    }
}