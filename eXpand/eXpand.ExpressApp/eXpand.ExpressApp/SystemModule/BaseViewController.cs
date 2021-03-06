﻿using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.NodeWrappers;

namespace eXpand.ExpressApp.SystemModule {
    public abstract partial class BaseViewController : ViewController {
        protected BaseViewController() {
            InitializeComponent();
            RegisterActions(components);
        }


        protected ClassInfoNodeWrapper GetClassInfoNodeWrapper() {
            return
                new ApplicationNodeWrapper(Application.Info).BOModel.Classes.Where(
                    wrapper => wrapper.ClassTypeInfo == View.ObjectTypeInfo).Single();
        }
    }
}