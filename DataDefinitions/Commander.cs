using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details about a commander
    /// </summary>
    public class Commander : INotifyPropertyChanged
    {
        /// <summary>The commander's name</summary>
        public string name { get; set;  }
        /// <summary>The commander's name as spoken</summary>
        public string phoneticname { get; set; }

        /// <summary> The commander's title.  This is dependent on the current system</summary>
        public string title { get; set; }

        /// <summary>The commander's combat rating</summary>
        public CombatRating combatrating { get; set; }

        /// <summary>The commander's trade rating</summary>
        public TradeRating traderating { get; set; }
        
        /// <summary>The commander's exploration rating</summary>
        public ExplorationRating explorationrating { get; set; }

        /// <summary>The commander's CQC rating</summary>
        public CQCRating cqcrating { get; set; }

        /// <summary>The commander's empire rating</summary>
        public EmpireRating empirerating { get; set; }

        /// <summary>The commander's federation rating</summary>
        public FederationRating federationrating { get; set; }

        /// <summary>The number of credits the commander holds</summary>
        public long credits { get; set; }
        /// <summary>The amount of debt the commander owes</summary>
        public long debt { get; set; }

        /// <summary>The insurance excess percentage the commander has to pay</summary>
        public decimal? insurance { get; set;  }
		

        private string _PPObedience;
		/// <summary>The PowerPlay Master choosen by the player</summary>
		public string powerplay
        {
            get
            {
                if (_PPObedience != null)
                {
                    //Logging.Info("Obedience" + _PPObedience);
                    return _PPObedience;
                }
                else
                {

                    //Logging.Info("Obedience" + "None");
                    return "None";
                }
            }
            set
            {
                if (_PPObedience != value)
                {
                    _PPObedience = value;
                    NotifyPropertyChanged("powerplay");
                }
            }
        }

        /// <summary>The commander's name</summary>
        public string gender { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
