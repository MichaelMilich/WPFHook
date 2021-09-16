using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WPFHook.Commands;
using WPFHook.Models;
using WPFHook.Views;
using WPFHook.ViewModels.BackgroundLogic;

namespace WPFHook.ViewModels
{
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

        public RuleViewModel()
        {

        }
        public RuleViewModel(List<RuleModel> ruleModels, TagViewModel tagViewModel)
        {
            this.tagViewModel = tagViewModel;
            foreach(RuleModel r in ruleModels)
            {
                _rules.Add(r);
            }
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
                /* string s = "Parmater Value = " + addRuleView.ruleParamters.Text + "\n"
                     + "Operator Value = " + addRuleView.ruleOperators.Text + "\n"
                     + "Constant value = " + addRuleView.constantTextBox.Text;
                 MessageBox.Show(s);
                */

                //first - make the strings according to the database requirements. maybe i should have a static function for that
                //Second - make checks like - the everything else rule can be only one, if someone makes it, we have to make sure there is still only one.
                //Further checks - make sure that there is no two rules with the same operator and the same constant for different tags.
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
                RuleModel newRule = new RuleModel(_rules[_rules.Count-1].RowId,ruleParameter, ruleOpertor, ruleConstant, selectedTag.TagID);
                SqliteDataAccess.saveRuleLast(newRule);
                UpdateRuleList(Tagger.BuildRules());
                addRuleView.Close();
            }
        }

    }
}
