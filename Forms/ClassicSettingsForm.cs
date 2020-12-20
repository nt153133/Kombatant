using System.Windows.Forms;

namespace Kombatant.Forms
{
	/// <summary>
	/// Description of ClassicSettingsForm.
	/// </summary>
	internal partial class ClassicSettingsForm : Form
	{
		internal ClassicSettingsForm()
		{
			InitializeComponent();
			InitializeLocalization();
			InitializeDataBinding();
		}

		/// <summary>
		/// Will bind the controls to the settings model.
		/// </summary>
		private void InitializeDataBinding()
		{
			// Convenience tab
			AddCheckboxBinding(chkAutoAcceptDutyFinder, @"AutoAcceptDutyFinder");
			AddCheckboxBinding(chkAutoAcceptQuests, @"AutoAcceptQuests");
			AddCheckboxBinding(chkAutoAdvanceDialogue, @"AutoAdvanceDialogue");
			AddCheckboxBinding(chkAutoEndActEncounters, @"AutoEndActEncounters");
			AddCheckboxBinding(chkAutoFaceTarget, @"AutoFaceTarget");
			AddCheckboxBinding(chkAutoSkipCutscenes, @"AutoSkipCutscenes");
			AddCheckboxBinding(chkAutoSprint, @"AutoSprint");
			AddCheckboxBinding(chkAutoSyncFate, @"AutoSyncFate");
			AddCheckboxBinding(chkDutyFinderNotify, @"DutyFinderNotify");
			AddCheckboxBinding(chkIsAutonomous, @"IsAutonomous");
			AddCheckboxBinding(chkIsPaused, @"IsPaused");
			AddCheckboxBinding(chkMechanicWarnings, @"MechanicWarnings");
			AddNumericBinding(numDutyFinderWaitTime, @"DutyFinderWaitTime");
		}

		/// <summary>
		/// Will set the labels and texts from the localized resource file.
		/// </summary>
		private void InitializeLocalization()
		{
			var toolTip = new ToolTip { ShowAlways = true };

			// Window title
			Text = Localization.Localization.UI_SettingsWindowTitle;

			// Tab headers
			tbpConvenience.Text = Localization.Localization.UI_Header_Convenience;
			tbpCrBehavior.Text = Localization.Localization.UI_Header_CombatRoutine;
			tbpHotkeys.Text = Localization.Localization.UI_Header_Hotkeys;
			tbpMovement.Text = Localization.Localization.UI_Header_Movement;
			tbpTargeting.Text = Localization.Localization.UI_Header_Targeting;

			// Convenience tab
			chkAutoAcceptDutyFinder.Text = Localization.Localization.UI_AutoAcceptDutyFinder;
			chkAutoAcceptQuests.Text = Localization.Localization.UI_AutoAcceptQuests;
			chkAutoAdvanceDialogue.Text = Localization.Localization.UI_AutoAdvanceDialogue;
			chkAutoEndActEncounters.Text = Localization.Localization.UI_AutoEndActEncounters;
			chkAutoFaceTarget.Text = Localization.Localization.UI_AutoFaceTarget;
			chkAutoSkipCutscenes.Text = Localization.Localization.UI_AutoSkipCutscenes;
			chkAutoSprint.Text = Localization.Localization.UI_AutoSprint;
			chkAutoSyncFate.Text = Localization.Localization.UI_AutoSyncFate;
			chkDutyFinderNotify.Text = Localization.Localization.UI_DutyFinderNotify;
			chkIsAutonomous.Text = Localization.Localization.UI_IsAutonomous;
			chkIsPaused.Text = Localization.Localization.UI_IsPaused;
			chkMechanicWarnings.Text = Localization.Localization.UI_MechanicWarnings;
			lblDutyFinderWaitTime.Text = Localization.Localization.UI_Lbl_DutyFinderWaitTime;

			toolTip.SetToolTip(chkAutoAcceptDutyFinder, Localization.Localization.UI_Tooltip_AutoAcceptDutyFinder);
			toolTip.SetToolTip(chkAutoAcceptQuests, Localization.Localization.UI_Tooltip_AutoAcceptQuests);
			toolTip.SetToolTip(chkAutoAdvanceDialogue, Localization.Localization.UI_Tooltip_AutoAdvanceDialogue);
			toolTip.SetToolTip(chkAutoEndActEncounters, Localization.Localization.UI_Tooltip_AutoEndActEncounters);
			toolTip.SetToolTip(chkAutoFaceTarget, Localization.Localization.UI_Tooltip_AutoFaceTarget);
			toolTip.SetToolTip(chkAutoSkipCutscenes, Localization.Localization.UI_Tooltip_AutoSkipCutscenes);
			toolTip.SetToolTip(chkAutoSprint, Localization.Localization.UI_Tooltip_AutoSprint);
			toolTip.SetToolTip(chkAutoSyncFate, Localization.Localization.UI_Tooltip_AutoSyncFate);
			toolTip.SetToolTip(chkDutyFinderNotify, Localization.Localization.UI_Tooltip_DutyFinderNotify);
			toolTip.SetToolTip(chkIsAutonomous, Localization.Localization.UI_Tooltip_IsAutonomous);
			toolTip.SetToolTip(chkIsPaused, Localization.Localization.UI_Tooltip_IsPaused);
			toolTip.SetToolTip(chkMechanicWarnings, Localization.Localization.UI_Tooltip_MechanicWarnings);
		}

		/// <summary>
		/// Helper method to encapsulate data bindings of CheckBox controls to the BotBase settings model.
		/// </summary>
		/// <param name="control">Control to bind</param>
		/// <param name="property">Property name within the BotBase settings model</param>
		private void AddCheckboxBinding(Control control, string property)
		{
			control.DataBindings.Add(@"Checked", Settings.BotBase.Instance, property, false,
				DataSourceUpdateMode.OnPropertyChanged);
		}

		/// <summary>
		/// Helper method to encapsulate data bindings of NumericUpDown controls to the BotBase settings model.
		/// </summary>
		/// <param name="control">Control to bind</param>
		/// <param name="property">Property name within the BotBase settings model</param>
		private void AddNumericBinding(NumericUpDown control, string property)
		{
			control.DataBindings.Add(@"Value", Settings.BotBase.Instance, property, false,
                DataSourceUpdateMode.OnPropertyChanged);
		}
	}
}
