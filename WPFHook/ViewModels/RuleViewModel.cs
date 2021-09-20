using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WPFHook.Commands;
using WPFHook.Models;
using WPFHook.Views;
using System.Linq;
using WPFHook.ViewModels.BackgroundLogic;

namespace WPFHook.ViewModels
{
    /// <summary>
    /// This class is the model view model for everything relevant to the rules.
    /// This is not the logic of the rules, but simply the class that connects the user to the available rules
    /// It does that using the AddRules window and DeleteRule Window (views). 
    /// More data - the rules have to know about the tags (hence the pointer to the tagViewModel).
    /// </summary>
    public class RuleViewModel
    {
        private TagViewModel tagViewModel;
        public TagViewModel TagViewModel { get { return tagViewModel; } }
        private ObservableCollection<RuleModel> _rules = new ObservableCollection<RuleModel>();
        public ObservableCollection<RuleModel> Rules
        {
            get
            {
                return _rules;
            }
            set
            {
                _rules = value;
            }
        }
        public AddRuleView addRuleView;
        public DeleteRuleWindow deleteRuleWindow;
        private ObservableCollection<RuleTagModel> ruleTag = new ObservableCollection<RuleTagModel>();
        /// <summary>
        /// Usefull Observable collection for the deleting Rules window. (showing the rule , the tag name and color in a single table).
        /// Other than that it was a good way to understand basic LINQ.
        /// </summary>
        public ObservableCollection<RuleTagModel> RuleTag
        {
            get
            {
                ruleTag.Clear();
                var query = from rule in _rules
                            join tag in tagViewModel.Tags on rule.TagId equals tag.TagID
                            select new
                            {
                                RuleId = rule.RowId,
                                Parameter = rule.Parameter,
                                Operator = rule.Operation,
                                Constant = rule.Constant,
                                TagName = tag.TagName,
                                TagColor = tag.TagColor
                            };
                foreach (var RuleTag in query)
                    ruleTag.Add(new RuleTagModel()
                    {
                        RuleId = RuleTag.RuleId,
                        Parameter = RuleTag.Parameter,
                        Constant = RuleTag.Constant,
                        Operator = RuleTag.Operator,
                        TagColor = RuleTag.TagColor,
                        TagName = RuleTag.TagName
                    });
                return ruleTag;
            }
        }

        public RuleViewModel()
        {

        }
        public RuleViewModel(List<RuleModel> ruleModels, TagViewModel tagViewModel)
        {
            this.tagViewModel = tagViewModel;
            tagViewModel.TagDeleted += TagViewModel_TagDeleted;
            foreach(RuleModel r in ruleModels)
            {
                _rules.Add(r);
            }
        }
        private void TagViewModel_TagDeleted(object sender, EventArgs e)
        {
            UpdateRuleList(Tagger.BuildRules());
        }

        private void UpdateRuleList(List<RuleModel> ruleModels)
        {
            _rules = new ObservableCollection<RuleModel>();
            foreach (RuleModel r in ruleModels)
            {
                _rules.Add(r);
            }
        }
        public ICommand AddRuleCommand { get { return new RelayCommand(e => true, this.AddRule); } }
        public ICommand OnDeleteRule { get { return new RelayCommand(e => true, this.DeleteRule); } }
        /// <summary>
        /// Writing into the Rules database.
        /// There window does not let the user input parameters that i don't want him too.
        /// There are minimal checks in code for duplicate rules because the checks are part of the database.
        /// NOTE FOR THE FUTURE - should place this under a try - exception clause.
        /// 
        /// after writing the rule into the database, have to update the Tagger functions.
        /// </summary>
        /// <param name="obj"></param>
        public void AddRule(object obj)
        {
            if (addRuleView.tagsComboBox.SelectedItem.Equals(tagViewModel.Tags[0]))
            {
                MessageBox.Show("Can't add rules to Computer time!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string ruleParameter = "";
                string ruleOpertor = "";
                string ruleConstant = addRuleView.constantTextBox.Text;
                TagModel selectedTag = (TagModel)addRuleView.tagsComboBox.SelectedItem;
                switch (addRuleView.ruleParamters.Text)
                {
                    case "":
                        //bad things
                        break;
                    case "Foreground Window Name":
                        ruleParameter = "FGWindowName";
                        break;
                    case "Foreground Process Name":
                        ruleParameter = "FGProcessName";
                        break;
                }
                if (addRuleView.ruleOperators.Text.Equals("Every thing else"))
                {
                    ruleOpertor = RuleModel.everythingElseRuleString;
                    ruleConstant = RuleModel.everythingElseRuleString;
                }
                else
                    ruleOpertor = addRuleView.ruleOperators.Text;
                RuleModel newRule = new RuleModel(0,ruleParameter, ruleOpertor, ruleConstant, selectedTag.TagID);
                SqliteDataAccess.saveRule(newRule);
                UpdateRuleList(Tagger.BuildRules());
                addRuleView.Close();
            }
        }
        /// <summary>
        /// When deleting a rule, we have to remove its function from the tagger.
        /// Note that after deleting the rule we update the tagger.
        /// </summary>
        /// <param name="obj"></param>
        public void DeleteRule(object obj)
        {
            if(deleteRuleWindow.RuleTagData.SelectedItem == null)
            {
                MessageBox.Show("Can't delete nothing :)", "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            else
            {
                var selected =(RuleTagModel) deleteRuleWindow.RuleTagData.SelectedItem;
                var query = from rules in _rules where rules.RowId == selected.RuleId select rules;
                RuleModel selectedRule=null;
                foreach (var r in query)
                    selectedRule = r;
                SqliteDataAccess.DeleteRule(selectedRule);
                _rules.Remove(selectedRule);
                ruleTag.Remove(selected);
                UpdateRuleList(Tagger.BuildRules());
                deleteRuleWindow.RuleTagData.Items.Refresh();
            }
        }
    }
}
