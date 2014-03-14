﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using moddingSuite.BL.Ndf;
using moddingSuite.Model.Edata;
using moddingSuite.Model.Ndfbin;
using moddingSuite.View.DialogProvider;
using moddingSuite.ViewModel.Base;
using moddingSuite.ViewModel.Edata;

namespace moddingSuite.ViewModel.Ndf
{
    public class NdfEditorMainViewModel : ViewModelBase
    {
        private readonly ObservableCollection<NdfClassViewModel> _classes =
            new ObservableCollection<NdfClassViewModel>();

        private ICollectionView _classesCollectionView;
        private string _classesFilterExpression = string.Empty;
        private string _statusText = string.Empty;

        private ICollectionView _stringCollectionView;
        private string _stringFilterExpression = string.Empty;
        private ObservableCollection<NdfStringReference> _strings;

        private ObservableCollection<NdfTranReference> _trans;
        private ICollectionView _transCollectionView;
        private string _transFilterExpression = string.Empty;

        public NdfEditorMainViewModel(EdataContentFile contentFile, EdataFileViewModel ownerVm)
        {
            OwnerFile = contentFile;
            EdataFileViewModel = ownerVm;

            var ndfbinReader = new NdfbinReader();
            NdfBinary = ndfbinReader.Read(ownerVm.EdataManager.GetRawData(contentFile));

            //var ndfbinManager = new NdfbinManager(ownerVm.EdataManager.GetRawData(contentFile));
            //NdfbinManager = ndfbinManager;

            //ndfbinManager.Initialize();

            InitializeNdfEditor();
        }

        private void InitializeNdfEditor()
        {
            foreach (NdfClass cls in NdfBinary.Classes)
                Classes.Add(new NdfClassViewModel(cls, this));

            Strings = NdfBinary.Strings;
            Trans = NdfBinary.Trans;

            SaveNdfbinCommand = new ActionCommand(SaveNdfbinExecute); //, () => NdfbinManager.ChangeManager.HasChanges);
            OpenInstanceCommand = new ActionCommand(OpenInstanceExecute);
            AddStringCommand = new ActionCommand(AddStringExecute);
            DeleteStringCommand = new ActionCommand(DeleteStringExecute);
        }


        /// <summary>
        ///     Virtual call
        /// </summary>
        /// <param name="content"></param>
        public NdfEditorMainViewModel(byte[] content)
        {
            OwnerFile = null;
            EdataFileViewModel = null;

            var ndfbinReader = new NdfbinReader();
            NdfBinary = ndfbinReader.Read(content);

            InitializeNdfEditor();

            SaveNdfbinCommand = new ActionCommand(SaveNdfbinExecute, () => false);
        }

        public NdfEditorMainViewModel(NdfBinary ndf)
        {
            NdfBinary = ndf;

            InitializeNdfEditor();

            SaveNdfbinCommand = new ActionCommand(SaveNdfbinExecute, () => false);
        }

        public NdfBinary NdfBinary { get; protected set; }

        protected EdataFileViewModel EdataFileViewModel { get; set; }
        protected EdataContentFile OwnerFile { get; set; }

        public ICommand SaveNdfbinCommand { get; set; }
        public ICommand OpenInstanceCommand { get; set; }
        public ICommand AddStringCommand { get; set; }
        public ICommand DeleteStringCommand { get; set; }

        public string Title
        {
            get
            {
                string path = "Virtual";

                if (OwnerFile != null)
                    path = OwnerFile.Path;

                return string.Format("Ndf Editor [{0}]", path);
            }
        }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged(() => StatusText);
            }
        }

        public string ClassesFilterExpression
        {
            get { return _classesFilterExpression; }
            set
            {
                _classesFilterExpression = value;
                OnPropertyChanged(() => ClassesFilterExpression);

                ClassesCollectionView.Refresh();
            }
        }

        public string StringFilterExpression
        {
            get { return _stringFilterExpression; }
            set
            {
                _stringFilterExpression = value;
                OnPropertyChanged(() => StringFilterExpression);
                StringCollectionView.Refresh();
            }
        }

        public string TransFilterExpression
        {
            get { return _transFilterExpression; }
            set
            {
                _transFilterExpression = value;
                OnPropertyChanged(() => TransFilterExpression);
                TransCollectionView.Refresh();
            }
        }

        public ICollectionView ClassesCollectionView
        {
            get
            {
                if (_classesCollectionView == null)
                {
                    BuildClassesCollectionView();
                }

                return _classesCollectionView;
            }
        }

        public ICollectionView StringCollectionView
        {
            get
            {
                if (_stringCollectionView == null)
                {
                    BuildStringCollectionView();
                }

                return _stringCollectionView;
            }
        }

        public ICollectionView TransCollectionView
        {
            get
            {
                if (_transCollectionView == null)
                {
                    BuildTransCollectionView();
                }

                return _transCollectionView;
            }
        }

        public ObservableCollection<NdfClassViewModel> Classes
        {
            get { return _classes; }
        }

        public ObservableCollection<NdfStringReference> Strings
        {
            get { return _strings; }
            set
            {
                _strings = value;
                OnPropertyChanged(() => Strings);
            }
        }

        public ObservableCollection<NdfTranReference> Trans
        {
            get { return _trans; }
            set
            {
                _trans = value;
                OnPropertyChanged(() => Trans);
            }
        }

        private void BuildClassesCollectionView()
        {
            _classesCollectionView = CollectionViewSource.GetDefaultView(Classes);
            _classesCollectionView.Filter = FilterClasses;

            OnPropertyChanged(() => ClassesCollectionView);
        }

        private void BuildStringCollectionView()
        {
            _stringCollectionView = CollectionViewSource.GetDefaultView(Strings);
            _stringCollectionView.Filter = FilterStrings;

            OnPropertyChanged(() => StringCollectionView);
        }

        private void BuildTransCollectionView()
        {
            _transCollectionView = CollectionViewSource.GetDefaultView(Trans);
            _transCollectionView.Filter = FilterTrans;

            OnPropertyChanged(() => TransCollectionView);
        }

        public bool FilterClasses(object o)
        {
            var clas = o as NdfClassViewModel;

            if (clas == null || ClassesFilterExpression == string.Empty)
                return true;

            string[] parts = ClassesFilterExpression.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);

            int cls;

            if (parts.Length > 1 && Int32.TryParse(parts[0], out cls) && (clas.Id == cls || clas.Name == parts[0]))
            {
                int inst;
                if (Int32.TryParse(parts[1], out inst))
                {
                    NdfObjectViewModel instObj = clas.Instances.SingleOrDefault(x => x.Id == inst);

                    if (instObj != null)
                        clas.InstancesCollectionView.MoveCurrentTo(instObj);
                }
            }

            return clas.Name.ToLower().Contains(parts[0].ToLower()) ||
                   clas.Id.ToString(CultureInfo.CurrentCulture).Contains(parts[0]) ||
                   clas.Instances.Any(x => x.Id.ToString(CultureInfo.InvariantCulture) == parts[0]);
        }

        public bool FilterStrings(object o)
        {
            var str = o as NdfStringReference;

            if (str == null || StringFilterExpression == string.Empty)
                return true;

            return str.Value.ToLower().Contains(StringFilterExpression.ToLower()) ||
                   str.Id.ToString(CultureInfo.CurrentCulture).Contains(StringFilterExpression);
        }

        public bool FilterTrans(object o)
        {
            var tran = o as NdfTranReference;

            if (tran == null || TransFilterExpression == string.Empty)
                return true;

            return tran.Value.ToLower().Contains(TransFilterExpression.ToLower()) ||
                   tran.Id.ToString(CultureInfo.CurrentCulture).Contains(TransFilterExpression);
        }

        private void SaveNdfbinExecute(object obj)
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Action<string> report = (msg) => StatusText = msg;

            var s = new Task(() =>
                {
                    try
                    {
                        dispatcher.Invoke(() => IsUIBusy = true);
                        dispatcher.Invoke(report, string.Format("Saving back changes..."));

                        byte[] newFile;

                        var writer = new NdfbinWriter();
                        using (var ms = new MemoryStream())
                        {
                            writer.Write(ms, NdfBinary, NdfBinary.Header.IsCompressedBody);
                            newFile = ms.ToArray();
                        }

                        dispatcher.Invoke(report, string.Format("Recompiling of {0} finished! ", EdataFileViewModel.EdataManager.FilePath));

                        EdataFileViewModel.EdataManager.ReplaceFile(OwnerFile, newFile);

                        dispatcher.Invoke(report, "Replacing new File in edata finished!");

                        EdataFileViewModel.LoadFile(EdataFileViewModel.LoadedFile);

                        EdataContentFile newOwen = EdataFileViewModel.EdataManager.Files.Single(x => x.Path == OwnerFile.Path);

                        OwnerFile = newOwen;
                        dispatcher.Invoke(report, string.Format("Saving of changes finished! {0}", EdataFileViewModel.EdataManager.FilePath));
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(string.Format("Error while saving Ndfbin file: {0}", ex));
                        dispatcher.Invoke(report, "Saving interrupted - Did you start Wargame before I was ready?");
                    }
                    finally
                    {
                        dispatcher.Invoke(() => IsUIBusy = false);
                    }
                });
            s.Start();
        }

        private void OpenInstanceExecute(object obj)
        {
            var cls = obj as NdfObjectViewModel;

            if (cls == null)
                return;

            var vm = new NdfClassViewModel(cls.Object.Class, this);

            NdfObjectViewModel inst = vm.Instances.SingleOrDefault(x => x.Id == cls.Id);

            if (inst == null)
                return;

            vm.InstancesCollectionView.MoveCurrentTo(inst);

            DialogProvider.ProvideView(vm, this);
        }

        private void DeleteStringExecute(object obj)
        {
            var cur = StringCollectionView.CurrentItem as NdfStringReference;

            if (cur == null)
                return;

            Strings.Remove(cur);
        }

        private void AddStringExecute(object obj)
        {
            Strings.Add(new NdfStringReference {Id = Strings.Count, Value = "<New string>"});
            StringCollectionView.MoveCurrentToLast();
        }
    }
}