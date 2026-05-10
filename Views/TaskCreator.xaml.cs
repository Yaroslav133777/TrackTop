using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
namespace TrackTop.Views;
public partial class TaskCreator : Popup
{
    public TaskCreator()
    {
        InitializeComponent();
        TaskTimePicker.Time = null;
        TaskDatePicker.Date = null;
    }
    
    private async void ClosePage(object? sender, EventArgs args)
    {
        await CloseAsync();
    }

    private async void CreateTask(object? sender, EventArgs args)
    {
        if (string.IsNullOrEmpty(TaskEditor.Text))
        {
            var toast = Toast.Make("Нужно ввести текст задачи!", ToastDuration.Long, 15);
            await toast.Show();
            return;
        }
        string description = TaskEditor.Text;
        
        if (TaskDatePicker.Date is null)
        {
            var toast = Toast.Make("Введите дату дедлайна задачи!", ToastDuration.Long, 15);
            await toast.Show();
            return;
        }
        
        if (TaskDatePicker.Date is not null)
        { 
            DateOnly date = DateOnly.FromDateTime((DateTime)TaskDatePicker.Date);
            
            if (date < DateOnly.FromDateTime(DateTime.Now))
            {
                var toast = Toast.Make("Дата дедлайна не может быть в прошлом!", ToastDuration.Long, 15);
                await toast.Show();
                return;
            }

            if (TaskTimePicker.Time is not null)
            {
                TimeOnly time = TimeOnly.FromTimeSpan((TimeSpan)TaskTimePicker.Time);
                if (time < TimeOnly.FromDateTime(DateTime.Now) && date == DateOnly.FromDateTime(DateTime.Now))
                {
                    var toast = Toast.Make("Время дедлайна не может быть в прошлом!", ToastDuration.Long, 15);
                    await toast.Show();
                    return;
                }
            }
            
        }
        
        
    }
    
}