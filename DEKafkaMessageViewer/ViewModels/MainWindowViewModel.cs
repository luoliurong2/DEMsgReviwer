using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rabbit.Zookeeper;
using System;
using Rabbit.Zookeeper.Implementation;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Windows.Threading;
using DEKafkaMessageViewer.Kafka;
using Confluent.Kafka;
using System.IO;
using System.Configuration;

namespace DEKafkaMessageViewer.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		#region properties

		private string _winTitle = "Data Exchange Kafka Messages Viewer";
		public string Title
		{
			get { return _winTitle; }
			set { SetProperty(ref _winTitle, value); }
		}

		private string _zookeeperHostServer = string.Empty;
		public string ZookeeperHostServer
		{
			get { return _zookeeperHostServer; }
			set { SetProperty(ref _zookeeperHostServer, value); }
		}

		private string _kafkaBrokerServer = string.Empty;
		public string KafkaHostServer
		{
			get { return _kafkaBrokerServer; }
			set { SetProperty(ref _kafkaBrokerServer, value); }
		}

		private string _kafkaConfigs = string.Empty;
		public string KafkaConfigs
		{
			get { return _kafkaConfigs; }
			set { SetProperty(ref _kafkaConfigs, value); }
		}

		private string _apiClassesPath = string.Empty;
		public string ApiClassesFilesPath
		{
			get { return _apiClassesPath; }
			set { SetProperty(ref _apiClassesPath, value); }
		}

		private string _selectedTopic;
		public string SelectedTopic
		{
			get { return _selectedTopic; }
			private set { SetProperty(ref _selectedTopic, value); }
		}

		public int TablesCount
		{
			get
			{
				return Tables.Count;
			}
		}

		public int MessagesCount
		{
			get
			{
				return ReceivedMessages.Count;
			}
		}

		private string _currentStatus;
		public string CurrentStatus
		{
			get { return _currentStatus; }
			set { SetProperty(ref _currentStatus, value); }
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

		#endregion
		private KafkaConsumer consumer = null;

		private DEKafkaMessageViewModel _selectedMessage;
		public DEKafkaMessageViewModel SelectedMessage
		{
			get { return _selectedMessage; }
			set { SetProperty(ref _selectedMessage, value); }
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
			}
		}

		private void OnSelectedTableRowsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

			Dispatcher.CurrentDispatcher.Invoke(() =>
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
			});
		}

		public DataGridViewModel DataGrid { get; private set; }

		public ObservableCollection<string> TopicItems { get; private set; }
		public ObservableCollection<DEKafkaMessageViewModel> ReceivedMessages { get; private set; }
		public ObservableCollection<TableViewModel> Tables { get; private set; }

        public DelegateCommand<object[]> TopicSelectedCommand { get; private set; }
		public DelegateCommand RetrieveZookeeperBrokerCommand { get; private set; }
		public DelegateCommand BrowseButtonCommand { get; private set; }
		public DelegateCommand VerifyAPIClassesCommand { get; private set; }
		public DelegateCommand StartConsumeCommand { get; private set; }
		public DelegateCommand StopConsumeCommand { get; private set; }
		public DelegateCommand ExecuteSearchCommand { get; private set; }

		public MainWindowViewModel()
		{
			_apiClassesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			TopicItems = new ObservableCollection<string>();
			//TopicItems.Add("==Select==");
			ReceivedMessages = new ObservableCollection<DEKafkaMessageViewModel>();
			Tables = new ObservableCollection<TableViewModel>();

            TopicSelectedCommand = new DelegateCommand<object[]>(OnItemSelected);
			RetrieveZookeeperBrokerCommand = new DelegateCommand(RetrieveZookeeperBrokerTopics);
			BrowseButtonCommand = new DelegateCommand(BrowseAPIClassesPath);
			VerifyAPIClassesCommand = new DelegateCommand(VeifyApiClassesAssembly);

			StartConsumeCommand = new DelegateCommand(StartConsumeMessages);
			StopConsumeCommand = new DelegateCommand(StopConsumeMessages, CanStopConsume);
			ExecuteSearchCommand = new DelegateCommand(ExecuteSearchMessages);

            InitializeWindowElementsValue();
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

		private void ExecuteSearchMessages() { }

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
				MessageBox.Show("Please specify the file path wher you stored the API classes!");
				return;
			}

			try
			{
				CurrentStatus = $"Verifying the API classes, just a moment...";
				DEKafkaMessageParser.InitializeEntityClassesTypes(ApiClassesFilesPath);
				CurrentStatus = $"Verification on the API classes has completed, all are good!";
			}
			catch (Exception ex)
			{
				CurrentStatus = $"API classes initialized failed!";
				MessageBox.Show(CurrentStatus + ex.Message);
			}
		}

		private async void StartConsumeMessages() {
			var canStart = !string.IsNullOrEmpty(ZookeeperHostServer) &&
					!string.IsNullOrEmpty(KafkaHostServer) &&
					TopicItems.Any() &&
					!string.IsNullOrEmpty(SelectedTopic);

			if (!canStart)
			{
				MessageBox.Show("Can't start consumer, please check zookeeper host, kafka host and selected topic!");
				return;
			}

			consumer = new KafkaConsumer(KafkaHostServer, SelectedTopic, KafkaConfigs);
			consumer.ConsumeError += HandleConsumeError;
			await Task.Factory.StartNew(() =>
			{
				try
				{
					CurrentStatus = $"Start consuming messages from kafka server '{KafkaHostServer}'";
					consumer.Subscribe(OnMessageConsumed);
				}
				catch(Exception ex)
				{
					CurrentStatus = $"Error occurs when cosuming messages. {ex.Message}";
					Stop();
				}
			});
		}

		private void HandleConsumeError(object sender, Error e)
		{
			CurrentStatus = $"Consumer encounters an error: {e.Reason}";
		}

		private void Stop()
		{
			if (consumer != null)
			{
				consumer.UnSubscribe();
				consumer = null;
				CurrentStatus = "Consume stopped.";
			}
		}

		private void OnMessageConsumed(object sender, Message<string, string> message)
		{
			if (message.Value.Trim().StartsWith($"<{DEKafkaMessageContract.DEKafkaMessage}>"))
			{
				var deKafkaMsg = DEKafkaMessageParser.DeSerializeDEKafkaMessage(message.Value);
				ShowDEKafkaMessageInfo(deKafkaMsg, message.Value);
			}
		}

		private void ShowDEKafkaMessageInfo(object msg, string rawXml)
		{
			DEKafkaMessageViewModel vm = new DEKafkaMessageViewModel(rawXml, msg);
			Dispatcher.CurrentDispatcher.Invoke(() =>
			{
				ReceivedMessages.Add(vm);
			});
			ShowChangedRecords(msg);
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
				Dispatcher.CurrentDispatcher.Invoke(() =>
				{
					Tables.Add(foundTable);
				});
			}
			foundTable.UpdateTableData(msgUnit);
		}

		private bool CanStopConsume()
		{
			return true;
		}

		private void StopConsumeMessages() {
			CurrentStatus = $"Stop consuming messages from kafka server '{KafkaHostServer}'";
			Stop();
		}

		private async void RetrieveZookeeperBrokerTopics()
		{
			if (string.IsNullOrEmpty(ZookeeperHostServer))
			{
				CurrentStatus = $"Zookeeper hosts is empty!";
				MessageBox.Show("Please specify at least one zookeeper server!");
				return;
			}
			if (!ValidateZookeeperSettings())
			{
				CurrentStatus = $"Zookeeper hosts specified is not valid!";
				MessageBox.Show("Zookeeper hosts specified is invalid! \r\n Please see format indicated in wartermark text!", "Zookeeper host is invalid");
				return;
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
							CurrentStatus = $"Zookeeper topics has been fetched.";
						});
					}
				}
				catch (Exception ex)
				{
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
        }
    }
}
