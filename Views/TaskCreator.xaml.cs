using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace TrackTop.Views;
public partial class TaskCreator : Popup
{
    private readonly AppStateService _appState;

    public TaskCreator(AppStateService appState)
    {
        _appState = appState;
        InitializeComponent();
        TaskTimePicker.Time = null;
        TaskDatePicker.Date = null;
        LoadFolders();
    }

    public void SetVoiceText(string text)
    {
        TaskEditor.Text = text;
    }

    private void LoadFolders()
    {
        FoldersPicker.Items.Clear();
        FoldersPicker.Items.Add("Без папки");
        foreach (var folder in _appState.Folders)
        {
            FoldersPicker.Items.Add(folder.FolderName);
        }
        FoldersPicker.SelectedIndex = 0;
    }

    private async void ClosePage(object? sender, EventArgs args)
    {
        await PopupCard.ScaleTo(0.9, 150, Easing.CubicIn);
        await PopupCard.FadeTo(0, 150, Easing.CubicIn);
        await CloseAsync();
    }

    private async void CreateTask(object? sender, EventArgs args)
    {
        if (string.IsNullOrWhiteSpace(TaskEditor.Text))
        {
            await Toast.Make("Нужно ввести текст задачи!", ToastDuration.Long, 15).Show();
            return;
        }
        // come to me
        string description = TaskEditor.Text;
        
        if (TaskDatePicker.Date is null)
        {
            await Toast.Make("Введите дату дедлайна задачи!", ToastDuration.Long, 15).Show();
            return;
        }
        
        if (TaskDatePicker.Date is not null)
        { 
            DateOnly date = DateOnly.FromDateTime((DateTime)TaskDatePicker.Date);
            
            if (date < DateOnly.FromDateTime(DateTime.Now))
            {
                await Toast.Make("Дата дедлайна не может быть в прошлом!", ToastDuration.Long, 15).Show();
                return;
            }
            
            if (TaskTimePicker.Time is not null)
            {
                TimeOnly time = TimeOnly.FromTimeSpan((TimeSpan)TaskTimePicker.Time);
                if (time < TimeOnly.FromDateTime(DateTime.Now) && date == DateOnly.FromDateTime(DateTime.Now))
                {
                    await Toast.Make("Время дедлайна не может быть в прошлом!", ToastDuration.Long, 15).Show();
                    return;
                }
            }
        }
        
        DateOnly dueDate = DateOnly.FromDateTime((DateTime)TaskDatePicker.Date!);
        TimeOnly? dueTime = TaskTimePicker.Time is not null 
            ? TimeOnly.FromTimeSpan((TimeSpan)TaskTimePicker.Time) 
            : null;
        
        bool notify = NotifySwitch.IsToggled;
        FolderModel? folder = null;
        
        if (FoldersPicker.SelectedIndex > 0 && FoldersPicker.SelectedIndex <= _appState.Folders.Count)
        {
            folder = _appState.Folders[FoldersPicker.SelectedIndex - 1];
        }
        
        var task = new TaskModel(description, dueDate, dueTime, notify, folder);
        
        if (folder != null)
        {
            folder.AddTask(task);
        }
        
        _appState.TodayTasks.Add(task);
        
        _appState.SaveData();
        
        await Toast.Make("Задача создана!", ToastDuration.Short, 15).Show();
        
        await PopupCard.ScaleTo(0.9, 150, Easing.CubicIn);
        await PopupCard.FadeTo(0, 150, Easing.CubicIn);
        await CloseAsync();
    }
}