﻿using DevExpress.ExpressApp.Win.Controls;
using DevExpress.XtraBars;

namespace eXpand.ExpressApp.Win.SystemModule
{
    public class HideToolBarController : ExpressApp.SystemModule.HideToolBarController{
        protected override void OnViewControlsCreated() {
            base.OnViewControlsCreated();
            var template = Frame.Template as IBarManagerHolder;
            if (template != null&&template.BarManager!=null) SetToolbarVisibility(template, !View.Info.GetAttributeBoolValue(HideToolBarAttributeName));
        }

        void SetToolbarVisibility(IBarManagerHolder template, bool visible){
            foreach (Bar bar in template.BarManager.Bars) {
                if (bar.BarName == "ListView Toolbar" || bar.BarName == "Main Toolbar"){
                    bar.Visible = visible;
                    break;
                }
            }
        }
    }
}
