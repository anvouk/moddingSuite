﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32;
using moddingSuite.BL.Ndf;
using moddingSuite.Model.Edata;
using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.View.DialogProvider;
using moddingSuite.ViewModel.Base;
using moddingSuite.ViewModel.Edata;

namespace moddingSuite.ViewModel.Ndf;

public class NdfEditorMainViewModel : ViewModelBase
{
    private ICollectionView _classesCollectionView;
    private string _classesFilterExpression = string.Empty;

    private List<NdfObject> _copyInstanceResults;
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

        NdfbinReader ndfbinReader = new NdfbinReader();
        NdfBinary = ndfbinReader.Read(ownerVm.EdataManager.GetRawData(contentFile));

        //var ndfbinManager = new NdfbinManager(ownerVm.EdataManager.GetRawData(contentFile));
        //NdfbinManager = ndfbinManager;

        //ndfbinManager.Initialize();

        InitializeNdfEditor();
    }

    /// <summary>
    ///     Virtual call
    /// </summary>
    /// <param name="content"></param>
    public NdfEditorMainViewModel(byte[] content)
    {
        OwnerFile = null;
        EdataFileViewModel = null;

        NdfbinReader ndfbinReader = new NdfbinReader();
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
    public ICommand FindAllReferencesCommand { get; set; }
    public ICommand CopyInstanceCommand { get; set; }
    public ICommand MakeTopObjectCommand { get; set; }
    public ICommand RunPythonScriptCommand { get; set; }

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
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged(() => StatusText);
        }
    }

    public string ClassesFilterExpression
    {
        get => _classesFilterExpression;
        set
        {
            _classesFilterExpression = value;
            OnPropertyChanged(() => ClassesFilterExpression);

            ClassesCollectionView.Refresh();
        }
    }

    public string StringFilterExpression
    {
        get => _stringFilterExpression;
        set
        {
            _stringFilterExpression = value;
            OnPropertyChanged(() => StringFilterExpression);
            StringCollectionView.Refresh();
        }
    }

    public string TransFilterExpression
    {
        get => _transFilterExpression;
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
            if (_classesCollectionView == null) BuildClassesCollectionView();

            return _classesCollectionView;
        }
    }

    public ICollectionView StringCollectionView
    {
        get
        {
            if (_stringCollectionView == null) BuildStringCollectionView();

            return _stringCollectionView;
        }
    }

    public ICollectionView TransCollectionView
    {
        get
        {
            if (_transCollectionView == null) BuildTransCollectionView();

            return _transCollectionView;
        }
    }

    public ObservableCollection<NdfClassViewModel> Classes { get; } = new();

    public ObservableCollection<NdfStringReference> Strings
    {
        get => _strings;
        set
        {
            _strings = value;
            OnPropertyChanged(() => Strings);
        }
    }

    public ObservableCollection<NdfTranReference> Trans
    {
        get => _trans;
        set
        {
            _trans = value;
            OnPropertyChanged(() => Trans);
        }
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

        FindAllReferencesCommand = new ActionCommand(FindAllReferencesExecute);
        CopyInstanceCommand = new ActionCommand(CopyInstanceExecute);
        MakeTopObjectCommand = new ActionCommand(MakeTopObjectExecute);

        RunPythonScriptCommand = new ActionCommand(RunPythonScript);
    }

    private void MakeTopObjectExecute(object obj)
    {
        NdfClassViewModel cls = ClassesCollectionView.CurrentItem as NdfClassViewModel;

        if (cls == null)
            return;

        NdfObjectViewModel inst = cls.InstancesCollectionView.CurrentItem as NdfObjectViewModel;

        if (inst == null)
            return;

        if (!inst.Object.IsTopObject)
        {
            NdfBinary.TopObjects.Add(inst.Object.Id);
            inst.Object.IsTopObject = true;
        }
    }

    private void CopyInstanceExecute(object obj)
    {
        NdfClassViewModel cls = ClassesCollectionView.CurrentItem as NdfClassViewModel;

        if (cls == null)
            return;

        NdfObjectViewModel inst = cls.InstancesCollectionView.CurrentItem as NdfObjectViewModel;

        if (inst == null)
            return;

        if (!inst.Object.IsTopObject)
        {
            MessageBox.Show("You can only create a copy of an top object.", "Information", MessageBoxButton.OK);
            return;
        }

        _copyInstanceResults = new List<NdfObject>();

        CopyInstance(inst.Object);

        ObjectCopyResultViewModel resultViewModel = new ObjectCopyResultViewModel(_copyInstanceResults, this);
        DialogProvider.ProvideView(resultViewModel, this);
    }

    private NdfObject CopyInstance(NdfObject instToCopy)
    {
        NdfObject newInst = instToCopy.Class.Manager.CreateInstanceOf(instToCopy.Class, instToCopy.IsTopObject);

        _copyInstanceResults.Add(newInst);

        foreach (NdfPropertyValue propertyValue in instToCopy.PropertyValues)
        {
            if (propertyValue.Type == NdfType.Unset)
                continue;

            NdfPropertyValue receiver = newInst.PropertyValues.Single(x => x.Property == propertyValue.Property);

            receiver.Value = GetCopiedValue(propertyValue);
        }

        instToCopy.Class.Instances.Add(newInst);

        NdfClassViewModel cls = Classes.SingleOrDefault(x => x.Object == instToCopy.Class);

        if (cls != null) cls.Instances.Add(new NdfObjectViewModel(newInst, cls.ParentVm));

        return newInst;
    }

    private NdfValueWrapper GetCopiedValue(IValueHolder toCopy)
    {
        NdfValueWrapper copiedValue = null;

        switch (toCopy.Value.Type)
        {
            case NdfType.ObjectReference:
                NdfObjectReference origInst = toCopy.Value as NdfObjectReference;

                if (origInst != null && !origInst.Instance.IsTopObject)
                    copiedValue = new NdfObjectReference(origInst.Class, CopyInstance(origInst.Instance).Id);
                else
                    copiedValue = NdfTypeManager.GetValue(toCopy.Value.GetBytes(), toCopy.Value.Type, toCopy.Manager);

                break;
            case NdfType.List:
            case NdfType.MapList:
                List<CollectionItemValueHolder> copiedItems = new List<CollectionItemValueHolder>();
                NdfCollection collection = toCopy.Value as NdfCollection;
                if (collection != null)
                    copiedItems.AddRange(collection.Select(entry =>
                        new CollectionItemValueHolder(GetCopiedValue(entry), toCopy.Manager)));
                copiedValue = new NdfCollection(copiedItems);
                break;

            case NdfType.Map:
                NdfMap map = toCopy.Value as NdfMap;
                if (map != null)
                    copiedValue = new NdfMap(new MapValueHolder(GetCopiedValue(map.Key), toCopy.Manager),
                        new MapValueHolder(GetCopiedValue(map.Value as IValueHolder), toCopy.Manager), toCopy.Manager);
                break;

            default:
                copiedValue = NdfTypeManager.GetValue(toCopy.Value.GetBytes(), toCopy.Value.Type, toCopy.Manager);
                break;
        }

        return copiedValue;
    }

    private void FindAllReferencesExecute(object obj)
    {
        NdfClassViewModel cls = ClassesCollectionView.CurrentItem as NdfClassViewModel;

        if (cls == null)
            return;

        NdfObjectViewModel inst = cls.InstancesCollectionView.CurrentItem as NdfObjectViewModel;

        if (inst == null)
            return;

        List<NdfPropertyValue> result = new List<NdfPropertyValue>();

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        Action<string> report = msg => StatusText = msg;

        Task s = new Task(() =>
        {
            try
            {
                //dispatcher.Invoke(() => IsUIBusy = true);
                dispatcher.Invoke(report, "Searching for references...");

                foreach (NdfObject instance in NdfBinary.Instances)
                foreach (NdfPropertyValue propertyValue in instance.PropertyValues)
                    GetValue(propertyValue, inst, result, propertyValue);

                ReferenceSearchResultViewModel resultVm = new ReferenceSearchResultViewModel(result, this);

                dispatcher.Invoke(() => DialogProvider.ProvideView(resultVm, this));
                dispatcher.Invoke(report, string.Format("{0} references found", result.Count));
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while saving Ndfbin file: {0}", ex);
                dispatcher.Invoke(report, "Error while searching");
            }
        });

        s.Start();
    }

    private void GetValue(IValueHolder valueHolder, NdfObjectViewModel inst, List<NdfPropertyValue> result,
        NdfPropertyValue propertyValue)
    {
        switch (valueHolder.Value.Type)
        {
            case NdfType.ObjectReference:
                NdfObjectReference ndfObjectReference = valueHolder.Value as NdfObjectReference;
                if (ndfObjectReference != null && ndfObjectReference.InstanceId == inst.Object.Id)
                    result.Add(propertyValue);
                break;
            case NdfType.List:
            case NdfType.MapList:
                NdfCollection ndfCollection = valueHolder.Value as NdfCollection;
                if (ndfCollection != null)
                    foreach (CollectionItemValueHolder col in ndfCollection)
                        GetValue(col, inst, result, propertyValue);
                break;
            case NdfType.Map:
                NdfMap map = valueHolder.Value as NdfMap;
                if (map != null)
                {
                    GetValue(map.Key, inst, result, propertyValue);
                    GetValue(map.Value as IValueHolder, inst, result, propertyValue);
                }

                break;
        }
    }

    private void RunPythonScript(object _)
    {
        OpenFileDialog scriptDlg = new OpenFileDialog
        {
            DefaultExt = ".py",
            Filter = "Python script (.py)|*.py|All Files|*.*"
        };

        if (scriptDlg.ShowDialog().Value)
        {
            ScriptEngine engine = Python.CreateEngine();
            engine.Runtime.LoadAssembly(Assembly.GetExecutingAssembly());
            ScriptScope scope = engine.CreateScope();
            scope.SetVariable("NdfBinary", NdfBinary);
            scope.SetVariable("Classes", new NdfScriptableClassList(Classes));
            try
            {
                engine.ExecuteFile(scriptDlg.FileName, scope);
            }
            catch (Exception e)
            {
                ExceptionOperations exceptionOps = engine.GetService<ExceptionOperations>();
                MessageBox.Show(exceptionOps.FormatException(e));
                return;
            }

            MessageBox.Show("Script successfully executed!");
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
        NdfClassViewModel clas = o as NdfClassViewModel;

        if (clas == null || ClassesFilterExpression == string.Empty)
            return true;

        string[] parts = ClassesFilterExpression.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

        int cls;

        if (parts.Length > 1 && int.TryParse(parts[0], out cls) && (clas.Id == cls || clas.Name == parts[0]))
        {
            int inst;
            if (int.TryParse(parts[1], out inst))
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
        NdfStringReference str = o as NdfStringReference;

        if (str == null || StringFilterExpression == string.Empty)
            return true;

        return str.Value.ToLower().Contains(StringFilterExpression.ToLower()) ||
               str.Id.ToString(CultureInfo.CurrentCulture).Contains(StringFilterExpression);
    }

    public bool FilterTrans(object o)
    {
        NdfTranReference tran = o as NdfTranReference;

        if (tran == null || TransFilterExpression == string.Empty)
            return true;

        return tran.Value.ToLower().Contains(TransFilterExpression.ToLower()) ||
               tran.Id.ToString(CultureInfo.CurrentCulture).Contains(TransFilterExpression);
    }

    private void SaveNdfbinExecute(object obj)
    {
        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        Action<string> report = msg => StatusText = msg;

        Task s = new Task(() =>
        {
            try
            {
                dispatcher.Invoke(() => IsUIBusy = true);
                dispatcher.Invoke(report, "Saving back changes...");

                NdfbinWriter writer = new NdfbinWriter();
                byte[] newFile = writer.Write(NdfBinary, NdfBinary.Header.IsCompressedBody);
                dispatcher.Invoke(report,
                    string.Format("Recompiling of {0} finished! ", EdataFileViewModel.EdataManager.FilePath));

                EdataFileViewModel.EdataManager.ReplaceFile(OwnerFile, newFile);
                dispatcher.Invoke(report, "Replacing new File in edata finished!");

                EdataFileViewModel.LoadFile(EdataFileViewModel.LoadedFile);

                EdataContentFile newOwen = EdataFileViewModel.EdataManager.Files.Single(x => x.Path == OwnerFile.Path);

                OwnerFile = newOwen;
                dispatcher.Invoke(report,
                    string.Format("Saving of changes finished! {0}", EdataFileViewModel.EdataManager.FilePath));
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while saving Ndfbin file: {0}", ex);
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
        NdfObjectViewModel cls = obj as NdfObjectViewModel;

        if (cls == null)
            return;
        NdfClassViewModel vm = new NdfClassViewModel(cls.Object.Class, this);
        NdfObjectViewModel inst = vm.Instances.SingleOrDefault(x => x.Id == cls.Id);
        ViewModelBase baseViewModel;
        switch (cls.Object.Class.Name)
        {
            case "TGameplayArmeArmureContainer":
            case "TGameplayDamageResistanceContainer":
                baseViewModel = new ArmourDamageViewModel(inst.Object, this);
                break;
            default:
                if (inst == null)
                    return;
                vm.InstancesCollectionView.MoveCurrentTo(inst);
                baseViewModel = vm;
                break;
        }

        DialogProvider.ProvideView(baseViewModel, this);
    }

    private void DeleteStringExecute(object obj)
    {
        NdfStringReference cur = StringCollectionView.CurrentItem as NdfStringReference;

        if (cur == null)
            return;

        Strings.Remove(cur);
    }

    private void AddStringExecute(object obj)
    {
        Strings.Add(new NdfStringReference { Id = Strings.Count, Value = "<New string>" });
        StringCollectionView.MoveCurrentToLast();
    }
}
