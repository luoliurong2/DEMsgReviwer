using DEKafkaMessageViewer.Kafka;
using Prism.Commands;
using Prism.Mvvm;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace DEKafkaMessageViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
	{
		#region properties

		private string _winTitle = "Data Exchange Kafka Messages Viewer";
		public string Title
		{
			get { return _winTitle; }
			set { SetProperty(ref _winTitle, value); RaisePropertyChanged("Title");}
		}

		private string _zookeeperHostServer = string.Empty;
		public string ZookeeperHostServer
		{
			get { return _zookeeperHostServer; }
			set { SetProperty(ref _zookeeperHostServer, value); RaisePropertyChanged("ZookeeperHostServer"); }
		}

		private string _kafkaBrokerServer = string.Empty;
		public string KafkaHostServer
		{
			get { return _kafkaBrokerServer; }
			set { SetProperty(ref _kafkaBrokerServer, value); RaisePropertyChanged("KafkaHostServer"); }
		}

		private string _kafkaConfigs = string.Empty;
		public string KafkaConfigs
		{
			get { return _kafkaConfigs; }
			set { SetProperty(ref _kafkaConfigs, value); RaisePropertyChanged("KafkaConfigs"); }
		}

		private string _apiClassesPath = string.Empty;
		public string ApiClassesFilesPath
		{
			get { return _apiClassesPath; }
			set { SetProperty(ref _apiClassesPath, value); RaisePropertyChanged("ApiClassesFilesPath"); }
		}

		private string _selectedTopic;
		public string SelectedTopic
		{
			get { return _selectedTopic; }
			private set {
                SetProperty(ref _selectedTopic, value);
                RaisePropertyChanged("ApiClassesFilesPath");
                RaisePropertyChanged("EnableStart");
            }
		}

		public int TablesCount
		{
			get
			{
				return Tables.Count;
			}
            set { RaisePropertyChanged("TablesCount"); }
		}

		public int MessagesCount
		{
			get
			{
				return ReceivedMessages.Count;
			}
            set { RaisePropertyChanged("MessagesCount"); }
		}

		private string _currentStatus;
		public string CurrentStatus
		{
			get { return _currentStatus; }
			set { SetProperty(ref _currentStatus, value); RaisePropertyChanged("CurrentStatus"); }
		}

		private bool _isFormatted;
		public bool IsFormatted
		{
			get { return _isFormatted; }
			set { SetProperty(ref _isFormatted, value); }
		}

		private string _searchText = string.Empty;
		public string SearchText
		{
			get { return _searchText; }
			set { SetProperty(ref _searchText, value); }
		}

        private bool _enableStart = false;
        public bool EnableStart
        {
            get {
                return !string.IsNullOrEmpty(ZookeeperHostServer) &&
                    !string.IsNullOrEmpty(KafkaHostServer) && 
                    TopicItems.Any() &&
                    !string.IsNullOrEmpty(SelectedTopic);
            }
            set { SetProperty(ref _enableStart, value); }
        }


        private bool _enableStop = false;
        public bool EnableStop
        {
            get { return _enableStop; }
            set { SetProperty(ref _enableStop, value); RaisePropertyChanged("EnableStop"); }
        }

        private bool _canSelectTopic = false;
        public bool CanSelectTopic
        {
            get { return _canSelectTopic; }
            set { SetProperty(ref _canSelectTopic, value); }
        }

        private bool _isFlyoutOpen = true;
        public bool IsFlyoutOpen
        {
            get { return _isFlyoutOpen; }
            set { SetProperty(ref _isFlyoutOpen, value); RaisePropertyChanged("IsFlyoutOpen"); }
        }

		private DEKafkaMessageViewModel _selectedMessage;
		public DEKafkaMessageViewModel SelectedMessage
		{
			get { return _selectedMessage; }
			set { SetProperty(ref _selectedMessage, value); RaisePropertyChanged("SelectedMessage"); }
		}

		private TableViewModel _selectedTable;
		public TableViewModel SelectedTable
		{
			get { return _selectedTable; }
			set
			{
				if (_selectedTable != null)
				{
					_selectedTable.Rows.CollectionChanged -= OnSelectedTableRowsCollectionChanged;
				}
				SetProperty(ref _selectedTable, value);

				DataGrid.Rows.Clear();
				DataGrid.Columns.Clear();
				if (value != null)
				{
					foreach (var column in value.Columns)
					{
						DataGrid.Columns.Add(column);
					}

					foreach (var row in value.Rows)
					{
						DataGrid.Rows.Add(row);
					}
				}
				if (_selectedTable != null)
				{
					_selectedTable.Rows.CollectionChanged += OnSelectedTableRowsCollectionChanged;
				}

                RaisePropertyChanged("SelectedTable");
			}
		}

		public DataGridViewModel DataGrid { get; private set; }

        private ObservableCollection<string> _topicItems = new ObservableCollection<string>();
        public ObservableCollection<string> TopicItems {
            get { return _topicItems; }
            set { SetProperty(ref _topicItems, value); }
        }

        private ObservableCollection<DEKafkaMessageViewModel> _receivedMessages = new ObservableCollection<DEKafkaMessageViewModel>();
        public ObservableCollection<DEKafkaMessageViewModel> ReceivedMessages {
            get { return _receivedMessages; }
            set { SetProperty(ref _receivedMessages, value); RaisePropertyChanged("ReceivedMessages"); }
        }

        private ObservableCollection<TableViewModel> _tables = new ObservableCollection<TableViewModel>();
        public ObservableCollection<TableViewModel> Tables {
            get { return _tables; }
            set { SetProperty(ref _tables, value); RaisePropertyChanged("Tables"); }
        }

        #endregion

        #region commands

        public DelegateCommand<object[]> TopicSelectedCommand { get; private set; }
        public DelegateCommand RetrieveZookeeperBrokerCommand { get; private set; }
		public DelegateCommand BrowseButtonCommand { get; private set; }
		public DelegateCommand VerifyAPIClassesCommand { get; private set; }
		public DelegateCommand StartConsumeCommand { get; private set; }
		public DelegateCommand StopConsumeCommand { get; private set; }
		public DelegateCommand ExecuteSearchCommand { get; private set; }
        public DelegateCommand SaveSettingsCommand { get; private set; }

        #endregion

        private Dispatcher CurrentDispatcher;

        public MainWindowViewModel()
		{
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
			_apiClassesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            TopicSelectedCommand = new DelegateCommand<object[]>(OnItemSelected);
            RetrieveZookeeperBrokerCommand = new DelegateCommand(RetrieveZookeeperBrokerTopics);
			BrowseButtonCommand = new DelegateCommand(BrowseAPIClassesPath);
			VerifyAPIClassesCommand = new DelegateCommand(VeifyApiClassesAssembly);

			StartConsumeCommand = new DelegateCommand(StartConsumeMessages);
			StopConsumeCommand = new DelegateCommand(StopConsumeMessages);
			ExecuteSearchCommand = new DelegateCommand(ExecuteSearchMessages);
            SaveSettingsCommand = new DelegateCommand(SaveSettingsConfig);

            DataGrid = new DataGridViewModel();
            ReceivedMessages.CollectionChanged += ReceivedMessages_Changed;
            Tables.CollectionChanged += Tables_Changed;
            InitializeWindowElementsValue();
		}

        #region private methods

        private void OnSelectedTableRowsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TableRowViewModel row in e.NewItems)
                {
                    DataGrid.Rows.Add(row);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (TableRowViewModel row in e.OldItems)
                {
                    if (DataGrid.Rows.Contains(row))
                    {
                        DataGrid.Rows.Remove(row);
                    }
                }
            }
        }

        private void ReceivedMessages_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("ReceivedMessages");
            MessagesCount = ReceivedMessages.Count;
        }

        private void Tables_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Tables");
            TablesCount = Tables.Count;
        }

        private void OnItemSelected(object[] selectedItems)
		{
			if (selectedItems != null && selectedItems.Any())
			{
				var selectedItem = selectedItems.FirstOrDefault();
				if (selectedItem != null)
				{
					SelectedTopic = selectedItem.ToString();
				}
			}
		}

        private void ExecuteSearchMessages() {
            try
            {
                //DataGrid.Search(SearchText);
            }
            catch
            {
                MessageBox.Show("The command is not supported. Please check the format.");
            }
        }

		private void BrowseAPIClassesPath() {
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			dialog.SelectedPath = _apiClassesPath;
			dialog.ShowNewFolderButton = false;
			var diaResult = dialog.ShowDialog();
			if (diaResult == DialogResult.OK)
			{
				ApiClassesFilesPath = dialog.SelectedPath;
			}
		}

		private void VeifyApiClassesAssembly()
		{
			if (string.IsNullOrEmpty(ApiClassesFilesPath))
			{
				CurrentStatus = $"API classes file path is empty!";
                settings_valid = false;
                MessageBox.Show("Please specify the file path wher you stored the API classes!");
				return;
			}

			try
			{
				CurrentStatus = $"Verifying the API classes, just a moment...";
				Task.Factory.StartNew(() =>
				{
					DEKafkaMessageParser.InitializeEntityClassesTypes(ApiClassesFilesPath);
				}).ContinueWith((result) =>
				{
					if (result.IsCompleted)
					{
						CurrentStatus = $"Verification on the API classes has completed, all are good!";
					}
				});
			}
			catch (Exception ex)
			{
                settings_valid = false;
                CurrentStatus = $"API classes initialized failed!";
				MessageBox.Show(CurrentStatus + ex.Message);
			}
		}

        private CancellationTokenSource _cancelConsume;
		private void StartConsumeMessages() {
			var canStart = !string.IsNullOrEmpty(ZookeeperHostServer) &&
					!string.IsNullOrEmpty(KafkaHostServer) &&
					TopicItems.Any() &&
					!string.IsNullOrEmpty(SelectedTopic);

			if (!canStart)
			{
				MessageBox.Show("Can't start consumer, please check zookeeper host, kafka host and selected topic!");
				return;
			}
            ReceivedMessages.Clear();
            Tables.Clear();
            _cancelConsume = new CancellationTokenSource();
            CurrentStatus = $"Consuming messages from topic:{SelectedTopic} ...";
            DEKafkaMessageViewer.Common.KafkaConsumer consumer = new Common.KafkaConsumer();
            var groupId = Guid.NewGuid().ToString();
            consumer.ConsumeAsync(KafkaHostServer, SelectedTopic, groupId, _cancelConsume, (resultMsg) =>
            {
                var msgBody = resultMsg.Message;
                EnableStop = true;
                OnMessageConsumed(msgBody);
            });
        }

		private void OnMessageConsumed(string message)
		{
			if (message.Trim().StartsWith($"<{DEKafkaMessageContract.DEKafkaMessage}>"))
			{
				var deKafkaMsg = DEKafkaMessageParser.DeSerializeDEKafkaMessage(message);
				ShowDEKafkaMessageInfo(deKafkaMsg, message);
			}
		}

		private void ShowDEKafkaMessageInfo(object msg, string rawXml)
		{
			DEKafkaMessageViewModel vm = new DEKafkaMessageViewModel(rawXml, msg);
            CurrentDispatcher.Invoke(() =>
            {
                ReceivedMessages.Add(vm);
                ShowChangedRecords(msg);
            });
        }

		private void ShowChangedRecords(object msg)
		{
			var batchUnits = msg.GetType().GetProperty(DEKafkaMessageContract.BatchUnits).GetValue(msg) as System.Collections.IEnumerable;
			foreach (var batchUnit in batchUnits) 
			{
				var messageUnits = batchUnit.GetType().GetProperty(DEKafkaMessageContract.MessageUnits).GetValue(batchUnit) as System.Collections.IEnumerable;
				foreach (var msgUnit in messageUnits)
				{
					ShowChangedRecord(msgUnit);
				}
			}
		}

		private void ShowChangedRecord(object msgUnit)
		{
			var objType = msgUnit.GetType();
			string classifierName = objType.GetProperty(DEKafkaMessageContract.ClassifierName).GetValue(msgUnit).ToString();
			string schemaName = objType.GetProperty(DEKafkaMessageContract.SchemaName).GetValue(msgUnit).ToString();
			var foundTable = Tables.FirstOrDefault(r => r.ClassifierName == classifierName && r.SchemaName == schemaName);

			if (foundTable == null)
			{
				foundTable = new TableViewModel(classifierName, schemaName);
                Tables.Add(foundTable);
            }
			foundTable.UpdateTableData(msgUnit);
		}

		private void StopConsumeMessages() {
            if (_cancelConsume != null)
            {
                _cancelConsume.Cancel();
                CurrentStatus = $"Stoped consuming messages from kafka server '{KafkaHostServer}'";
                EnableStop = false;
            }
		}

        private bool settings_valid = true;
		private async void RetrieveZookeeperBrokerTopics()
		{
			if (string.IsNullOrEmpty(ZookeeperHostServer))
			{
				CurrentStatus = $"Zookeeper hosts is empty!";
				MessageBox.Show("Please specify at least one zookeeper server!");
                settings_valid = false;
                return;
			}
			if (!ValidateZookeeperSettings())
			{
				CurrentStatus = $"Zookeeper hosts specified is not valid!";
                settings_valid = false;
                MessageBox.Show("Zookeeper hosts specified is invalid! \r\n Please see format indicated in wartermark text!", "Zookeeper host is invalid");
				return;
			}

			if (TopicItems.Any())
			{
				TopicItems.Clear();
			}

			using (IZookeeperClient client = new ZookeeperClient(new ZookeeperClientOptions(ZookeeperHostServer)
			{
				BasePath = "/", //default value
				ConnectionTimeout = TimeSpan.FromSeconds(10), //default value
				SessionTimeout = TimeSpan.FromSeconds(20), //default value
				OperatingTimeout = TimeSpan.FromSeconds(60), //default value
				ReadOnly = false, //default value
				SessionId = 0, //default value
				SessionPasswd = null, //default value
				EnableEphemeralNodeRestore = true //default value
			}))
			{
				CurrentStatus = $"Retrieving data from Zookeeper server {ZookeeperHostServer}.";
				try
				{
					var childNodes = await client.GetChildrenAsync("/brokers/topics");
					if (childNodes.Any())
					{
						Dispatcher.CurrentDispatcher.Invoke(() => {
							TopicItems.AddRange(childNodes);
                            CanSelectTopic = true;
							CurrentStatus = $"Zookeeper topics has been fetched successfully.";
						});
					}
				}
				catch (Exception ex)
				{
                    settings_valid = false;
                    TopicItems.Clear();
					CurrentStatus = $"Error occurs when communicating with Zookeeper '{ZookeeperHostServer}'. {ex.Message}";
				}
				finally
				{
					client.Dispose();
				}
			}
		}

		private bool ValidateZookeeperSettings()
		{
			if (string.IsNullOrEmpty(ZookeeperHostServer))
			{
				return false;
			}
			var idx = ZookeeperHostServer.IndexOf(':');
			if (idx <= 0)
			{
				return false;
			}
			var commaIdx = ZookeeperHostServer.IndexOf(',');
			if (commaIdx > 0)
			{
				var zookeepers = ZookeeperHostServer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var zookeeper in zookeepers)
				{
					idx = zookeeper.IndexOf(':');
					if (idx <= 0)
					{
						return false;
					}
					if (!ValidateHostAndPort(zookeeper.Substring(0, idx), zookeeper.Substring(idx + 1)))
					{
						return false;
					}
				}
				return true;
			}
			else
			{
				return ValidateHostAndPort(ZookeeperHostServer.Substring(0, idx), ZookeeperHostServer.Substring(idx + 1));
			}
		}

		private bool ValidateHostAndPort(string host, string port)
		{
			if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
			{
				return false;
			}

			var portNum = 0;
			if (!int.TryParse(port, out portNum))
			{
				return false;
			}

			return true;
		}

        private void InitializeWindowElementsValue()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var zookeeperHost = appSettings.Get("ZookeeperBootstraper");
            if (!string.IsNullOrEmpty(zookeeperHost))
            {
                ZookeeperHostServer = zookeeperHost;
            }
            var kafkaHost = appSettings.Get("KafkaBootstraper");
            if (!string.IsNullOrEmpty(kafkaHost))
            {
                KafkaHostServer = kafkaHost;
            }
            var otherConfigs = appSettings.Get("KafkaConfigs");
            if (!string.IsNullOrEmpty(otherConfigs))
            {
                KafkaConfigs = otherConfigs;
            }
            var apiClassLocation = appSettings.Get("APIClassesLocation");
            if (!string.IsNullOrEmpty(apiClassLocation))
            {
                ApiClassesFilesPath = apiClassLocation;
            }

            var secretKey = appSettings.Get("SecretKey");
            var successKey = Encrypt(encodedKey, Encoding.UTF8);
            if (!string.IsNullOrEmpty(secretKey) && secretKey == successKey)
            {
                IsFlyoutOpen = false;
                RetrieveZookeeperBrokerTopics();
                VeifyApiClassesAssembly();
            }
        }

        private void SaveSettingsConfig()
        {
            if (string.IsNullOrEmpty(ZookeeperHostServer))
            {
                MessageBox.Show("Zookeeper host server must not be empty.");
                return;
            }
            if (string.IsNullOrEmpty(KafkaHostServer))
            {
                MessageBox.Show("Kafka host server must not be empty.");
                return;
            }
            if (string.IsNullOrEmpty(ApiClassesFilesPath))
            {
                MessageBox.Show("API class path must not be empty.");
                return;
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["ZookeeperBootstraper"].Value = ZookeeperHostServer;
            config.AppSettings.Settings["KafkaBootstraper"].Value = KafkaHostServer;
            config.AppSettings.Settings["APIClassesLocation"].Value = ApiClassesFilesPath;
            if (!string.IsNullOrEmpty(KafkaConfigs))
            {
                config.AppSettings.Settings["KafkaConfigs"].Value = KafkaConfigs;
            }
            if (settings_valid)
            {
                config.AppSettings.Settings["SecretKey"].Value = Encrypt(encodedKey, Encoding.UTF8);
            }

            try
            {
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                CurrentStatus = "Settings have been saved successfully!";
				IsFlyoutOpen = false;
			}
            catch (Exception ex)
            {
                MessageBox.Show($"Errors occur when saving settings! Error: {ex.Message}");
            }
        }
        private readonly string encodedKey = "success";
        private string Encrypt(string text, Encoding encoding)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                var textbytes = encoding.GetBytes(text);
                var computedHash = md5.ComputeHash(textbytes);
                var result = BitConverter.ToString(computedHash);
                return result.Replace("-", "");
            }
        }

        #endregion
    }
}
