using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR3DCameraDemo.ViewModel
{
    class Point3DViewModel : NotificationObject
    {
        private int iD;

        public int ID
        {
            get { return iD; }
            set
            {
                iD = value;
                this.RaisePropertyChanged("ID");
            }
        }
        private double x;

        public double X
        {
            get { return x; }
            set
            {
                x = value;
                this.RaisePropertyChanged("X");
            }
        }
        private double y;

        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                this.RaisePropertyChanged("Y");
            }
        }
        private double z;

        public double Z
        {
            get { return z; }
            set
            {
                z = value;
                this.RaisePropertyChanged("Z");
            }
        }
        private double dist;

        public double Dist
        {
            get { return dist; }
            set
            {
                dist = value;
                this.RaisePropertyChanged("Dist");
            }
        }

    }
}
