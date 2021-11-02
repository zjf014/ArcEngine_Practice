using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AE开发实例
{
    class ControlsRemoveLayer:BaseCommand
    {
        IMapControl3 mapcontrol;

        public override void OnCreate(object hook)
        {
            //throw new NotImplementedException();
            mapcontrol = (IMapControl3)hook;
            base.m_caption = "移除图层";
        }

        public override void OnClick()
        {
            ILayer layer;
            layer = mapcontrol.CustomProperty;
            mapcontrol.Map.DeleteLayer(layer);
        }

    }
}
