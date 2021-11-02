using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace AE开发实例
{
    class ControlsMapFreeSelect:ITool,ICommand
    {
        IMapControl2 mapControl;

        public int Cursor
        {
            get { //throw new NotImplementedException(); 
                return (int)ESRI.ArcGIS.Controls.esriControlsMousePointer.esriPointerPencil;
            }
        }

        public bool Deactivate()
        {
            //throw new NotImplementedException();
            return true;
        }

        public bool OnContextMenu(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void OnDblClick()
        {
            throw new NotImplementedException();
        }

        public void OnKeyDown(int keyCode, int shift)
        {
            throw new NotImplementedException();
        }

        public void OnKeyUp(int keyCode, int shift)
        {
            throw new NotImplementedException();
        }

        public void OnMouseDown(int button, int shift, int x, int y)
        {
            //throw new NotImplementedException();
            ESRI.ArcGIS.Carto.ILayer layer;
            layer = (ESRI.ArcGIS.Carto.ILayer)mapControl.Map.get_Layer(0);

            ESRI.ArcGIS.Carto.IFeatureSelection featureSelection = layer as ESRI.ArcGIS.Carto.IFeatureSelection;

            IPolygon pPolygon = (IPolygon)mapControl.TrackPolygon();

            ESRI.ArcGIS.Geodatabase.ISpatialFilter spatialFilter = new ESRI.ArcGIS.Geodatabase.SpatialFilter();
            spatialFilter.Geometry = pPolygon;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            featureSelection.SelectFeatures(spatialFilter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultNew, false);

            mapControl.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        public void OnMouseMove(int button, int shift, int x, int y)
        {
            //throw new NotImplementedException();
        }

        public void OnMouseUp(int button, int shift, int x, int y)
        {
            //throw new NotImplementedException();
        }

        public void Refresh(int hdc)
        {
            //throw new NotImplementedException();
        }

        public int Bitmap
        {
            get { throw new NotImplementedException(); }
        }

        public string Caption
        {
            get { throw new NotImplementedException(); }
        }

        public string Category
        {
            get { throw new NotImplementedException(); }
        }

        public bool Checked
        {
            get { throw new NotImplementedException(); }
        }

        public bool Enabled
        {
            get { throw new NotImplementedException(); }
        }

        public int HelpContextID
        {
            get { throw new NotImplementedException(); }
        }

        public string HelpFile
        {
            get { throw new NotImplementedException(); }
        }

        public string Message
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public void OnClick()
        {
            //throw new NotImplementedException();
        }

        public void OnCreate(object Hook)
        {
            //throw new NotImplementedException();
            mapControl = (IMapControl2)Hook;
        }

        public string Tooltip
        {
            get { throw new NotImplementedException(); }
        }
    }
}
