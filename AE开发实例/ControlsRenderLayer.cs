using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;

namespace AE开发实例
{
    class ControlsRenderLayer : BaseCommand
    {
        IMapControl3 mapcontrol;

        public override void OnCreate(object hook)
        {
            //throw new NotImplementedException();
            mapcontrol = (IMapControl3)hook;
            base.m_caption = "渲染图层";
        }

        public override void OnClick()
        {
            IFeatureLayer layer = (IFeatureLayer)mapcontrol.CustomProperty;

            IRgbColor color = new RgbColor();
            color.Red = 255;
            color.Blue = 0;
            color.Green = 0;

            ICharacterMarkerSymbol charMarkersymbol = new CharacterMarkerSymbol();
            //charMarkersymbol.Font = Converter.ToStdFont(new Font(new FontFamily("ESRI Default Marker"), 12.0f, FontStyle.Regular));
            charMarkersymbol.CharacterIndex = 96;
            charMarkersymbol.Size = 12.0;
            charMarkersymbol.Color = (IColor)color;


            IRandomColorRamp randomColorRamp = new RandomColorRamp();
            randomColorRamp.MinSaturation = 20;
            randomColorRamp.MaxSaturation = 40;
            randomColorRamp.MaxValue = 85;
            randomColorRamp.MaxValue = 100;
            randomColorRamp.StartHue = 75;
            randomColorRamp.EndHue = 190;
            randomColorRamp.UseSeed = true;
            randomColorRamp.Seed = 45;

            IUniqueValueRenderer uniqueRenderer = new UniqueValueRenderer();
            uniqueRenderer.FieldCount = 1;
            uniqueRenderer.set_Field(0, "FID");
            uniqueRenderer.DefaultSymbol = (ISymbol)charMarkersymbol;
            uniqueRenderer.UseDefaultSymbol = true;


            IFeatureClass featureClass = layer.FeatureClass;

            Random rand = new Random();
            bool bValFound = false;
            IFeatureCursor featureCursor = featureClass.Search(null, true);
            IFeature feature = null;
            string val = string.Empty;
            int fieldID = featureClass.FindField("FID");
            //if (-1 == fieldID)
            //    return uniqueRenderer;

            while ((feature = featureCursor.NextFeature()) != null)
            {
                bValFound = false;
                val = Convert.ToString(feature.get_Value(fieldID));
                for (int i = 0; i < uniqueRenderer.ValueCount - 1; i++)
                {
                    if (uniqueRenderer.get_Value(i) == val)
                        bValFound = true;
                }

                if (!bValFound)//need to add the value to the renderer
                {
                    color.Red = rand.Next(255);
                    color.Blue = rand.Next(255);
                    color.Green = rand.Next(255);

                    charMarkersymbol = new CharacterMarkerSymbol();
                    //charMarkersymbol.Font = Converter.ToStdFont(new Font(new FontFamily("ESRI Default Marker"), 10.0f, FontStyle.Regular));
                    charMarkersymbol.CharacterIndex = rand.Next(40, 118);
                    charMarkersymbol.Size = 20.0;
                    charMarkersymbol.Color = (IColor)color;

                    //add the value to the renderer
                    uniqueRenderer.AddValue(val, "name", (ISymbol)charMarkersymbol);
                }
            }

            IGeoFeatureLayer gLayer = (IGeoFeatureLayer)layer;
            gLayer.Renderer = (IFeatureRenderer)uniqueRenderer;
            mapcontrol.ActiveView.Refresh();
        }
    }
}
