﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Business_Logic_Layer;
using Database.Entity;
using System.IO;
using System.Threading;

namespace RemindMe
{
    public partial class MUCImportExport : UserControl
    {
        private ReminderTransferType transferType;
        private List<Reminder> remindersFromRemindMeFile = new List<Reminder>();
        public MUCImportExport()
        {
            InitializeComponent();
        }

        private void cbAction_SelectedIndexChanged(object sender, EventArgs e)
        {            
            switch (cbAction.SelectedItem.ToString())
            {
                case "Export reminders":
                    LoadExportReminders();
                    break;
                case "Import reminders":
                    LoadImportReminders();
                    break;
                case "Recover deleted reminders":
                    LoadDeletedReminders();
                    break;
                case "Recover old reminders":
                    LoadOldReminders();
                    break;

            }
            if (lvReminders.Items.Count > 17)
                chTitle.Width = 414;
            else
                chTitle.Width = 430;
        }
     
        private void LoadOldReminders()
        {
            BLIO.Log("Recover old button pressed. Loading reminders into listview");
            
            lvReminders.Items.Clear();
            transferType = ReminderTransferType.RECOVER;

            BLFormLogic.AddRemindersToListview(lvReminders, BLReminder.GetArchivedReminders().OrderBy(rem => rem.Name).ToList(), true);
            BLIO.Log("Added old reminders to listview.");
        }
        private void LoadDeletedReminders()
        {
            BLIO.Log("Recover deleted button pressed. Loading reminders into listview");

            lvReminders.Items.Clear();
            transferType = ReminderTransferType.RECOVER;

            BLFormLogic.AddRemindersToListview(lvReminders, BLReminder.GetDeletedReminders().OrderBy(rem => rem.Name).ToList(), true);
            BLIO.Log("Added deleted reminders to listview.");
        }
        private void LoadExportReminders()
        {
            BLIO.Log("(MUCImportExport)btnExport_Click");
            lvReminders.Items.Clear();            

            BLIO.Log("Export button pressed. Loading reminders into listview");
            if (BLReminder.GetReminders().Count > 0)
            {
                transferType = ReminderTransferType.EXPORT;
                BLFormLogic.AddRemindersToListview(lvReminders, BLReminder.GetReminders(), true);
            }
            BLIO.Log("Added exportable reminders to listview.");
        }
        private void LoadImportReminders()
        {
            BLIO.Log("Import button pressed. Loading reminders into listview");
            remindersFromRemindMeFile.Clear();
            lvReminders.Items.Clear();

            string remindmeFile = FSManager.Files.GetSelectedFileWithPath("RemindMe backup file", "*.remindme");

            if (remindmeFile == null || remindmeFile == "")
            {//user pressed cancel                                 
                return;
            }
            BLIO.Log("Valid .remindme file selected");

            try
            {



                List<object> toImportReminders = BLReminder.DeserializeRemindersFromFile(remindmeFile).Cast<object>().ToList();


                if (toImportReminders != null)
                {
                    BLIO.Log(toImportReminders.Count - 1 + " reminders in this .remindme file");
                    transferType = ReminderTransferType.IMPORT;

                    foreach (object rem in toImportReminders)
                    {
                        if (rem.GetType() == typeof(Reminder))
                        {
                            BLFormLogic.AddReminderToListview(lvReminders, (Reminder)rem, true);
                            remindersFromRemindMeFile.Add((Reminder)rem);
                        }
                    }
                }

            }
            catch
            {
                MaterialMessageFormManager.MakeMessagePopup("Error loading reminder(s)", 6);
            }
        }
           

        private void btnCancel_Click(object sender, EventArgs e)
        {
            BLIO.Log("Cancel button pressed (MUCImportExport)");            
            lvReminders.Items.Clear();

        }



        private void btnConfirm_Click(object sender, EventArgs e)
        {
            BLIO.Log("Confirm button pressed (MUCImportExport)");
            bool success = false;
            switch (transferType)
            {
                case ReminderTransferType.IMPORT:
                    BLIO.Log("Importing reminders ... (MUCImportExport)");
                    success = ImportReminders();
                    break;
                case ReminderTransferType.EXPORT:
                    BLIO.Log("Exporting reminders ... (MUCImportExport)");
                    success = Exportreminders();
                    break;
                case ReminderTransferType.RECOVER:
                    BLIO.Log("Recovering reminders ... (MUCImportExport)");
                    success = RecoverReminders();
                    break;
            }
            if (success)
                foreach (ListViewItem item in lvReminders.SelectedItems)
                    lvReminders.Items.Remove(item);

            
            MUCReminders.Instance.UpdateCurrentPage();
        }





        private bool Exportreminders()
        {
            if (GetSelectedRemindersFromListview().Count > 0)
            {
                string selectedPath = FSManager.Folders.GetSelectedFolderPath();

                if (selectedPath != null)
                {
                    BLIO.Log("User selected a valid path");

                    Exception possibleException = BLReminder.ExportReminders(GetSelectedRemindersFromListview(), selectedPath);
                    if (possibleException == null)
                    {
                        BLIO.Log("No problems encountered (exception null)");
                        SetStatusTexts(GetSelectedRemindersFromListview().Count, BLReminder.GetReminders().Count);
                    }
                    else if (possibleException is UnauthorizedAccessException)
                    {
                        BLIO.Log("Problem encountered: Unauthorized");
                        if (MaterialRemindMeBox.Show("Could not save reminders to \"" + selectedPath + "\"\r\nDo you wish to place them on your desktop instead?", RemindMeBoxReason.YesNo) == DialogResult.Yes)
                        {
                            BLIO.Log("Trying to save to desktop instead...");
                            possibleException = BLReminder.ExportReminders(GetSelectedRemindersFromListview(), Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                            if (possibleException != null)
                            {//Did saving to desktop go wrong, too?? just show a message
                                BLIO.Log("Trying to save to desktop didnt work either");
                                MaterialRemindMeBox.Show("Something went wrong. Could not save the reminders to your desktop.", RemindMeBoxReason.OK);
                                return false;
                            }
                            else
                            {//Saving to desktop did not throw an exception      
                                BLIO.Log("Saved to desktop");
                                SetStatusTexts(GetSelectedRemindersFromListview().Count, BLReminder.GetReminders().Count);
                            }
                        }
                    }
                    else
                    {
                        MaterialMessageFormManager.MakeMessagePopup("Backup failed.", 6);
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            else
            {
                MaterialMessageFormManager.MakeMessagePopup("Please select one or more reminder(s)", 6);
            }
            return true;

        }
        private bool ImportReminders()
        {
            int remindersInserted = 0;
            List<Reminder> selectedReminders = GetSelectedRemindersFromListview();

            if (selectedReminders.Count == 0)
                return false;



            if (remindersFromRemindMeFile != null)
            {
                BLIO.Log("Attempting to import " + selectedReminders.Count + " reminders ...");
                foreach (Reminder rem in selectedReminders)
                {
                    if (!File.Exists(rem.SoundFilePath)) //when you import reminders on another device, the path to the file might not exist. remove it.
                        rem.SoundFilePath = "";


                    BLReminder.PushReminderToDatabase(rem);
                    BLIO.Log("Pushed reminder with id " + rem.Id + " to the database");
                    remindersInserted++;
                }

                new Thread(() =>
                {
                    //Log an entry to the database, for data!                                                
                    try
                    {
                        BLOnlineDatabase.ImportCount++;
                    }
                    catch (ArgumentException ex)
                    {
                        BLIO.Log("Exception at BLOnlineDatabase.ImportCount++ MUCImportExport.cs . -> " + ex.Message);
                        BLIO.WriteError(ex, ex.Message, true);
                    }
                }).Start();

                BLIO.Log(remindersInserted + " Reminders inserted");
            }
            SetStatusTexts(remindersInserted, selectedReminders.Count);
            return true;
        }
        private bool RecoverReminders()
        {
            int remindersRecovered = 0;
            List<Reminder> selectedReminders = GetSelectedRemindersFromListview();

            if (selectedReminders.Count == 0)
                return false;

            BLIO.Log("Attempting to recover " + selectedReminders.Count + " reminders ...");
            foreach (Reminder rem in selectedReminders)
            {
                if (!File.Exists(rem.SoundFilePath)) //when you import reminders on another device, the path to the file might not exist. remove it.
                    rem.SoundFilePath = "";

                if (rem.Deleted == 1 || rem.Deleted == 2) //The user wants to recover reminders, instead of importing new ones
                {
                    BLIO.Log("Reminder deleted: " + rem.Deleted + ". Setting deleted,enabled and hidden to 0");
                    rem.Deleted = 0;
                    rem.Enabled = 0; //Disable it so the user doesnt instantly get the reminder as an popup, as the reminder was in the past
                    rem.Hide = 0;    //Make sure it isn't hidden, since you cant easily re-enable hidden reminders, you first have to unhide all reminders first
                    BLReminder.EditReminder(rem);
                    BLIO.Log("Reminder with id " + rem.Id + " edited");
                }
                remindersRecovered++;
            }

            new Thread(() =>
            {
                //Log an entry to the database, for data!                
                try
                {
                    BLOnlineDatabase.RecoverCount++;
                }
                catch (ArgumentException ex)
                {
                    BLIO.Log("Exception at BLOnlineDatabase.RecoverCount++. -> " + ex.Message);
                    BLIO.WriteError(ex, ex.Message, true);
                }
            }).Start();

            BLIO.Log(remindersRecovered + " Reminders recovered");
            SetStatusTexts(remindersRecovered, selectedReminders.Count);
            return true;
        }


        private List<Reminder> GetSelectedRemindersFromListview()
        {
            List<long> checkedIds = new List<long>(); //get all selected id's from the listview reminders
            foreach (ListViewItem item in lvReminders.SelectedItems)
                checkedIds.Add((long)item.Tag);

            if (transferType == ReminderTransferType.IMPORT) //Look through the reminders from the .remindme file instead of the database if import.
                return remindersFromRemindMeFile.Where(r => checkedIds.Contains(r.Id)).ToList();
            else
                return BLReminder.GetAllReminders().Where(r => checkedIds.Contains(r.Id)).ToList();
        }

        private void SetStatusTexts(int completedReminders, int totalReminders)
        {
            foreach (ListViewItem item in lvReminders.SelectedItems)
                lvReminders.Items.Remove(item);

            if (completedReminders > 0)
                MaterialMessageFormManager.MakeMessagePopup("Succesfully " + this.transferType.ToString().ToLower() + "ed " + completedReminders + " reminders.", 4);
        }


        private void lvReminders_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                //Ctrl+a = select all items
                foreach (ListViewItem item in lvReminders.Items)
                    item.Selected = true;
            }
        }

        /// <summary>
        /// Determines what the user wants to do with reminders. import,export or recover
        /// </summary>
        private enum ReminderTransferType
        {
            /// <summary>
            /// The user wants to import new reminders
            /// </summary>
            IMPORT,
            /// <summary>
            /// The user wants to export his reminders
            /// </summary>
            EXPORT,
            /// <summary>
            /// The user wants to recover his previously deleted reminders
            /// </summary>
            RECOVER
        }

    

      

        private void MUCImportExport_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
                BLIO.Log("Control MUCImportExport now visible");
        }


       
    }


}