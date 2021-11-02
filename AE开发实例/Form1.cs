using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AE开发实例
{
    public partial class Form1 : Form
    {
        private string m_mapDocumentName = string.Empty;
        private ILayer m_layer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            axTOCControl1.SetBuddyControl(axMapControl1.Object);
            保存文档ToolStripMenuItem.Enabled = false;

            FillTreeView();
        }

        private void FillTreeView()
        {
            treeView1.Nodes.Clear();
            TreeNode tn = treeView1.Nodes.Add("本地数据");
            //treeView1.Nodes.Add(tn);
            FillNode(tn, 1);
        }

        private void FillNode(TreeNode treeNode, int level)
        { 
            string[] paths = null;
            int iindex = -1;
            try
            {
                if (treeNode.Text == "本地数据")
                {
                    paths = System.IO.Directory.GetLogicalDrives();
                }
                else
                {
                    paths = System.IO.Directory.GetFileSystemEntries(treeNode.Tag.ToString());
                }
                foreach (string path in paths)
                {
                    if (treeNode.Level > level) break;
                    if (System.IO.Directory.Exists(path))
                    {
                        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
                        if ((di.Attributes & System.IO.FileAttributes.Hidden) != 0 && path.Length > 3) continue;
                        iindex = 0;
                    }
                    else if (System.IO.File.Exists(path))
                    {
                        //System.IO.FileAttributes att = System.IO.File.GetAttributes(path);
                        if ((System.IO.File.GetAttributes(path) & System.IO.FileAttributes.Hidden) != 0) continue;
                        //if ((System.IO.File.GetAttributes(path) & System.IO.FileAttributes.Archive) != 0) continue;

                        if (System.IO.Path.GetExtension(path).ToUpper() != ".SHP") continue;

                        iindex = 1;
                    }
                    long count = path.Split('\\').LongLength;
                    string fn;
                    if (count == 2)
                        if (path.Split('\\')[1] == "")
                            fn = path;
                        else
                            fn = path.Split('\\')[1];
                    else
                        fn = path.Split('\\')[count - 1];
                    TreeNode tn = treeNode.Nodes.Add(fn, fn, iindex);
                    tn.Tag = path;
                    FillNode(tn, level);

                    // 实际使用时下面的if块要删除掉，这里使用的目的显示少一些的内容，让程序运行速度快一些
                    //if (tn.Nodes.Count > 5)
                    //{
                    //    break;
                    //}

                }
            }
            catch
            { }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            e.Node.Nodes.Clear();
            FillNode(e.Node, e.Node.Level + 1);
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.ImageIndex != 1) return;
            long length = e.Node.Text.Split('.').LongLength;
            switch (e.Node.Text.Split('.')[length - 1].ToUpper())
            {
                //case "MXD":
                //    axMapControl1.LoadMxFile(e.Node.Tag.ToString());
                //    break;
                case "SHP":
                    string path1 = e.Node.Tag.ToString();
                    string path2 = System.IO.Path.GetDirectoryName(path1);

                    IPropertySet ps = new PropertySet();
                    IWorkspaceFactory pFact;
                    IWorkspace pWorkspace;
                    ps.SetProperty("DATABASE", path2);
                    pFact = new ShapefileWorkspaceFactory();
                    pWorkspace = pFact.Open(ps, 0);
                    IFeatureWorkspace pFW = (IFeatureWorkspace)pWorkspace;
                    IFeatureClass pFC = pFW.OpenFeatureClass(System.IO.Path.GetFileNameWithoutExtension(path1));
                    IFeatureLayer pFL = new FeatureLayer();
                    pFL.FeatureClass = pFC;
                    pFL.Name = pFC.AliasName;
                    axMapControl1.AddLayer(pFL);
                    break;
                //case "JPG":
                //    break;
            }
        }

        private void axMapControl1_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e)
        {
            toolStripStatusLabel1.Text = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
            
        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 新建文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //execute New Document command
            ICommand command = new CreateNewDocument();
            command.OnCreate(axMapControl1.Object);
            command.OnClick();
        }

        private void 打开文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //execute Open Document command
            ICommand command = new ControlsOpenDocCommand();
            command.OnCreate(axMapControl1.Object);
            command.OnClick();
        }

        private void 保存文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //execute Save Document command
            if (axMapControl1.CheckMxFile(m_mapDocumentName))
            {
                //create a new instance of a MapDocument
                IMapDocument mapDoc = new MapDocument();
                mapDoc.Open(m_mapDocumentName, string.Empty);

                //Make sure that the MapDocument is not readonly
                if (mapDoc.get_IsReadOnly(m_mapDocumentName))
                {
                    MessageBox.Show("Map document is read only!");
                    mapDoc.Close();
                    return;
                }

                //Replace its contents with the current map
                mapDoc.ReplaceContents((IMxdContents)axMapControl1.Map);

                //save the MapDocument in order to persist it
                mapDoc.Save(mapDoc.UsesRelativePaths, false);

                //close the MapDocument
                mapDoc.Close();
            }
        }

        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            //get the current document name from the MapControl
            m_mapDocumentName = axMapControl1.DocumentFilename;

            //if there is no MapDocument, diable the Save menu and clear the statusbar
            if (m_mapDocumentName == string.Empty)
            {
                保存文档ToolStripMenuItem.Enabled = false;
                toolStripStatusLabel1.Text = string.Empty;
            }
            else
            {
                //enable the Save manu and write the doc name to the statusbar
                保存文档ToolStripMenuItem.Enabled = true;
                toolStripStatusLabel1.Text = System.IO.Path.GetFileName(m_mapDocumentName);
            }
        }

        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ICommand command = new ControlsSaveAsDocCommand();
            command.OnCreate(axMapControl1.Object);
            command.OnClick();
        }

        private void 通用添加数据方法ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICommand command = new ControlsAddDataCommand();
            command.OnCreate(axMapControl1.Object);
            command.OnClick();
        }

        private void 添加栅格数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IGxDialog dlg = new GxDialog();
            IGxObjectFilterCollection filtercollection = dlg as IGxObjectFilterCollection;
            filtercollection.AddFilter(new GxFilterRasterDatasets(), true);
            filtercollection.AddFilter(new GxFilterRasterCatalogDatasets(), true);
            IEnumGxObject enumobj;
            
            dlg.AllowMultiSelect = true;
            dlg.Title = "添加栅格数据";
            dlg.DoModalOpen(0, out enumobj);
            if (enumobj != null)
            {
                ILayer layer;
                enumobj.Reset();
                IGxObject gxobj = enumobj.Next();
                while (gxobj != null)
                {
                    if (gxobj is IGxDataset)
                    {
                        IGxDataset gxdataset = gxobj as IGxDataset;
                        IDataset pdataset = gxdataset.Dataset;
                        switch (pdataset.Type)
                        {
                            case esriDatasetType.esriDTRasterDataset:
                                IRasterDataset rasterds = pdataset as IRasterDataset;
                                IRasterLayer rasterlayer = new RasterLayer();
                                rasterlayer.CreateFromDataset(rasterds);
                                layer = rasterlayer;
                                layer.Name = rasterlayer.Name;
                                axMapControl1.Map.AddLayer(layer);
                                break;
                            default:
                                break;
                        }
                    }
                    else if (gxobj is IGxLayer)
                    {
                        IGxLayer gxlayer = gxobj as IGxLayer;
                        layer = gxlayer.Layer;
                        layer.Name = gxlayer.Layer.Name;
                        axMapControl1.Map.AddLayer(layer);
                    }
                    gxobj = enumobj.Next();
                }
            }
        }

        private void 添加矢量数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IGxDialog gd = new GxDialog();
            IGxObjectFilter goFilter = new GxFilterShapefiles();
            IEnumGxObject go = null;
            
            IFeatureLayer layer;
            IGxDataset gxDataset = null;

            gd.AllowMultiSelect = true;
            gd.Title = "加载shp图层";
            gd.ButtonCaption = "添加图层";
            gd.ObjectFilter = goFilter;
            if (gd.DoModalOpen(0, out go))
            {
                if (go != null)
                {
                    go.Reset();
                    gxDataset = go.Next() as IGxDataset;
                }

                while (gxDataset != null)
                {
                    layer = new FeatureLayer();
                    layer.FeatureClass = gxDataset.Dataset as IFeatureClass;
                    layer.Name = layer.FeatureClass.AliasName;
                    axMapControl1.AddLayer(layer);
                    gxDataset = go.Next() as IGxDataset;
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ILayer[] maplys = new ILayer[axMapControl1.LayerCount];
            ILayer[] scenelys = new ILayer[axSceneControl1.Scene.LayerCount];
            for (int i = 0; i < axMapControl1.LayerCount; i++)
                maplys[i] = axMapControl1.get_Layer(i);
            for (int i = 0; i < axSceneControl1.Scene.LayerCount; i++)
                scenelys[i] = axSceneControl1.Scene.get_Layer(i);


            if (tabControl1.SelectedIndex == 0)
            {
                axTOCControl1.SetBuddyControl(axMapControl1);
                axToolbarControl1.Enabled = true;
                axToolbarControl1.SetBuddyControl(axMapControl1.Object);
                axToolbarControl2.Enabled = false;

                foreach (ILayer maply in scenelys)
                {
                    if (isExist(maply, maplys) == false) axMapControl1.AddLayer(maply);
                }    
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                axSceneControl1.Scene.Name = axMapControl1.Map.Name;
                axSceneControl1.Scene.SpatialReference = axMapControl1.SpatialReference;

                axTOCControl1.SetBuddyControl(axSceneControl1);
                axToolbarControl2.Enabled = true;
                axToolbarControl2.SetBuddyControl(axSceneControl1.Object);
                axToolbarControl1.Enabled = false;

                foreach (ILayer maply in maplys)
                {
                    if (isExist(maply, scenelys) == false) axSceneControl1.Scene.AddLayer(maply, false);
                }               
                
                IActiveView pActiveView1 = this.axMapControl1.Map as IActiveView;
                IEnvelope enve = pActiveView1.Extent as IEnvelope;

                IPoint point = new ESRI.ArcGIS.Geometry.Point();        //将此区域的中心点保存起来
                point.X = (enve.XMax + enve.XMin) / 2;  //取得视角中心点X坐标
                point.Y = (enve.YMax + enve.YMin) / 2;  //取得视角中心点Y坐标

                IPoint ptTaget = new ESRI.ArcGIS.Geometry.Point();      //创建一个目标点
                ptTaget = point;        //视觉区域中心点作为目标点
                ptTaget.Z = 0;         //设置目标点高度，这里设为 0米

                IPoint ptObserver = new ESRI.ArcGIS.Geometry.Point();   //创建观察点的X，Y，Z
                ptObserver.X = point.X;     //设置观察点坐标的X坐标
                ptObserver.Y = point.Y;    //设置观察点坐标的Y坐标（这里加90米，是在南北方向上加了90米，当然这个数字可以自己定，意思就是将观察点和目标点有一定的偏差，从南向北观察

                double height = (enve.Width < enve.Height) ? enve.Width : enve.Height;      //计算观察点合适的高度，这里用三目运算符实现的，效果稍微好一些，当然可以自己拟定
                ptObserver.Z = height;              //设置观察点坐标的Y坐标

                ICamera pCamera = this.axSceneControl1.Camera;      //取得三维活动区域的Camara ，就像你照相一样的视角，它有Taget（目标点）和Observer（观察点）两个属性需要设置    
                pCamera.Target = ptTaget;       //赋予目标点
                pCamera.Observer = ptObserver;      //将上面设置的观察点赋予camera的观察点
                pCamera.Zoom(1.4);
                pCamera.Inclination = 30;       //设置三维场景视角，也就是高度角，视线与地面所成的角度
                pCamera.Azimuth = 225;          //设置三维场景方位角，视线与向北的方向所成的角度

                axSceneControl1.Scene.SceneGraph.RefreshViewers();
            }
        }

        private bool isExist(System.Object mlayer, System.Array mlayers)
        {
            foreach (System.Object player in mlayers)
            {
                if (player == mlayer) return true;
            }
            return false;
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null;
            m_layer = null;
            object other = null;
            object index = null;
            axTOCControl1.HitTest(e.x, e.y, ref item, ref map, ref m_layer, ref other, ref index);//判断所点击的类型

            if (e.button == 1 && m_layer != null)
            {
                if (m_layer is IFeatureLayer)
                {
                    //dataGridView1.Rows.Clear();
                    IFeatureLayer pFeaturelayer = (IFeatureLayer)m_layer;
                    IFeatureCursor pFC = pFeaturelayer.Search(null, true);
                    IFeature pFeature = pFC.NextFeature();
                    int j = 0;
                    DataTable dt = new DataTable();
                    for (int i = 0; i < pFeaturelayer.FeatureClass.Fields.FieldCount; i++)
                    {
                        IField pField = pFeaturelayer.FeatureClass.Fields.get_Field(i);
                        DataColumn pDC = new DataColumn(pField.Name);
                        if (pField.Name == pFeaturelayer.FeatureClass.OIDFieldName) pDC.Unique = true;
                        pDC.AllowDBNull = pField.IsNullable;
                        pDC.Caption = pField.AliasName;
                        pDC.DefaultValue = pField.DefaultValue;
                        if (pField.VarType == 8) pDC.MaxLength = pField.Length;
                        dt.Columns.Add(pDC);
                    }
                    while (pFeature != null)
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < pFeaturelayer.FeatureClass.Fields.FieldCount; i++)
                        {
                            if (pFeature.Fields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                                dr[i] = pFeature.Shape.GeometryType.ToString();
                            else
                                dr[i] = pFeature.get_Value(i);
                        }
                        dt.Rows.Add(dr);
                        pFeature = pFC.NextFeature();
                        j++;
                    }
                    dataGridView1.DataSource = dt;

                }
            }
        }

        private void axMapControl1_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            if (e.button == 2)
            {
                contextMenuStrip1.Show(axMapControl1, e.x, e.y);
            }
        }

        private void 全景ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICommand cmd = new ControlsMapFullExtentCommand();
            cmd.OnCreate(axMapControl1.Object);
            cmd.OnClick();
        }

        private void 拉框放大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICommand cmd = new ControlsMapZoomInTool();
            cmd.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = cmd as ITool;
        }

        private void 按比例放大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICommand cmd = new ControlsMapFixedZoomIn();
            cmd.OnCreate(axMapControl1.Object);
            cmd.OnClick();
        }

        private void 自由选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICommand cmd = new ControlsMapFreeSelect();
            cmd.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = cmd as ITool;
        }

        private void axTOCControl1_OnMouseUp(object sender, ITOCControlEvents_OnMouseUpEvent e)
        {
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null;
            m_layer = null;
            object other = null;
            object index = null;
            axTOCControl1.HitTest(e.x, e.y, ref item, ref map, ref m_layer, ref other, ref index);//判断所点击的类型

            if (e.button == 2)
            {
                if (item == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    axMapControl1.CustomProperty = m_layer;

                    IToolbarMenu menu = new ToolbarMenu();
                    menu.SetHook(axMapControl1.Object);

                    menu.AddItem(new ControlsFeatureSelectionToolbar());
                    menu.AddSubMenu(new ControlsFeatureSelectionMenu());
                    menu.AddItem(new ControlsRemoveLayer());
                    menu.AddItem(new ControlsRenderLayer());

                    menu.PopupMenu(e.x, e.y, axTOCControl1.hWnd);
                    
                }
 
            }
        }

        private void 创建shpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace workspace = workspaceFactory.OpenFromFile(@"C:\Users\zjf01\Desktop\DATA", 0);
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;

            IFields fields = new Fields();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            IField field = new Field();
            IFieldEdit fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "SHAPE";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            IGeometryDef geoDef = new GeometryDef();
            IGeometryDefEdit geoDefEdit = (IGeometryDefEdit)geoDef;
            geoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            geoDefEdit.SpatialReference_2 = (ISpatialReference)new UnknownCoordinateSystem();
            fieldEdit.GeometryDef_2 = geoDef;

            fieldsEdit.AddField(field);

            field = new Field();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "id";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.Length_2 = 10;

            fieldsEdit.AddField(field);

            IFeatureClass featureClass = featureWorkspace.CreateFeatureClass("newPoint.shp", fields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

            IFeatureLayer featureLyr = new FeatureLayer();
            featureLyr.FeatureClass = featureClass;
            featureLyr.Name = "newPoint";
            axMapControl1.AddLayer(featureLyr);
        }

        private void 生成中心点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IFeatureLayer polygonLayer = axMapControl1.get_Layer(1) as IFeatureLayer;
            IFeatureClass polygonLayerClass = polygonLayer.FeatureClass;
            IFeatureLayer pointLayer = axMapControl1.get_Layer(0) as IFeatureLayer;
            IFeatureClass pointLayerClass = pointLayer.FeatureClass;

            IDataset dataset = (IDataset)pointLayerClass;
            IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace workspace = workspaceFactory.OpenFromFile(dataset.Workspace.PathName, 0);
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;

            IFeatureCursor cursor = polygonLayer.FeatureClass.Search(null, false);
            IFeature feature = cursor.NextFeature();

            workspaceEdit.StartEditOperation();
            workspaceEdit.StartEditing(true);

            while (feature != null)
            {
                IPoint point = new ESRI.ArcGIS.Geometry.Point();
                IArea area = (IArea)feature.Shape;
                area.QueryCentroid(point);
                IFeature newFeature = pointLayerClass.CreateFeature();
                newFeature.Shape = point;
                newFeature.Store();

                feature = cursor.NextFeature();
            }

            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);

            axMapControl1.ActiveView.Refresh();
        }

        private void 按照属性查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ILayer layer = (ILayer)axMapControl1.Map.get_Layer(0);

            IFeatureSelection featureSelection = layer as IFeatureSelection;

            IQueryFilter filter = new QueryFilter();
            filter.WhereClause = "NAME='辽宁'";

            featureSelection.SelectFeatures(filter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultNew, false);

            axMapControl1.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        private void 按照位置查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ILayer layer = (ILayer)axMapControl1.Map.get_Layer(0);

            IFeatureSelection featureSelection = layer as IFeatureSelection;

            IFeatureLayer featureLyr = (IFeatureLayer)layer;
            IQueryFilter filter = new QueryFilter();
            filter.WhereClause = "NAME='辽宁'";
            IFeatureCursor cursor = featureLyr.Search(filter, false);
            IFeature feature = cursor.NextFeature();
            if (feature != null)
            {
                ESRI.ArcGIS.Geodatabase.ISpatialFilter spatialFilter = new ESRI.ArcGIS.Geodatabase.SpatialFilter();
                spatialFilter.Geometry = feature.Shape;
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                featureSelection.SelectFeatures(spatialFilter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultNew, false);

                axMapControl1.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
        }

        private void bufferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();

            ESRI.ArcGIS.AnalysisTools.Buffer buffer = new ESRI.ArcGIS.AnalysisTools.Buffer();

            buffer.in_features = axMapControl1.get_Layer(0);
            buffer.out_feature_class = "C:\\Users\\zjf01\\Desktop\\DATA\\buffer.shp";
            buffer.buffer_distance_or_field = "1000000000 meters";

            gp.Execute(buffer, null);

            for (int i = 0; i < gp.MessageCount; i++)
                MessageBox.Show(gp.GetMessage(i));

            //string file = (string)buffer.out_feature_class;
            //string path = System.IO.Path.GetDirectoryName(file);
            //string filename = System.IO.Path.GetFileNameWithoutExtension(file);

            axMapControl1.AddShapeFile("C:\\Users\\zjf01\\Desktop\\DATA", "buffer.shp");
        }

        private void intersectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            ESRI.ArcGIS.AnalysisTools.Intersect intersect = new ESRI.ArcGIS.AnalysisTools.Intersect();

            ESRI.ArcGIS.Geoprocessing.IGpValueTableObject go = new ESRI.ArcGIS.Geoprocessing.GpValueTableObject();
            go.SetColumns(2);
            go.AddRow(axMapControl1.get_Layer(0));
            go.AddRow(axMapControl1.get_Layer(1));

            intersect.in_features = go;
            intersect.out_feature_class = System.IO.Directory.GetCurrentDirectory() + "\\intersect.shp";

            try
            {
                gp.Execute(intersect, null);
            }
            catch
            {
                for (int i = 0; i < gp.MessageCount; i++)
                    MessageBox.Show(gp.GetMessage(i));
            }
        }

        private void bufferToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer = (IFeatureLayer)axMapControl1.get_Layer(0);
            IFeature feature = featureLayer.FeatureClass.GetFeature(0);
            IPoint point = new ESRI.ArcGIS.Geometry.Point();
            point = (IPoint)feature.Shape;

            ITopologicalOperator to = (ITopologicalOperator)point;
            IGeometry buffer = to.Buffer(1000000000);

            IGraphicsContainer gc = (IGraphicsContainer)axMapControl1.Map;
            IElement element = new PolygonElement();
            element.Geometry = buffer;
            gc.AddElement(element, 0);
            axMapControl1.ActiveView.Refresh();
        }

        private void intersectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer1 = (IFeatureLayer)axMapControl1.get_Layer(0);
            IFeature feature1 = featureLayer1.FeatureClass.GetFeature(0);
            IPolygon polygon1 = (IPolygon)feature1.Shape;

            IFeatureLayer featureLayer2 = (IFeatureLayer)axMapControl1.get_Layer(1);
            IFeature feature2 = featureLayer2.FeatureClass.GetFeature(0);
            IPolygon polygon2 = (IPolygon)feature2.Shape;

            ITopologicalOperator to = (ITopologicalOperator)polygon1;
            IGeometry intersect = to.Intersect(polygon2, esriGeometryDimension.esriGeometry2Dimension);

            IGraphicsContainer gc = (IGraphicsContainer)axMapControl1.Map;
            IElement element = new PolygonElement();
            element.Geometry = intersect;
            gc.AddElement(element, 0);
            axMapControl1.ActiveView.Refresh();
        }

        private void overlapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer1 = (IFeatureLayer)axMapControl1.get_Layer(0);
            IFeature feature1 = featureLayer1.FeatureClass.GetFeature(0);
            IPolygon polygon1 = (IPolygon)feature1.Shape;

            IFeatureLayer featureLayer2 = (IFeatureLayer)axMapControl1.get_Layer(1);
            IFeature feature2 = featureLayer2.FeatureClass.GetFeature(0);
            IPolygon polygon2 = (IPolygon)feature2.Shape;

            IRelationalOperator ro = (IRelationalOperator)polygon1;

            bool bl = ro.Overlaps(polygon2);
            MessageBox.Show(bl.ToString());
        }

        private void relationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer1 = (IFeatureLayer)axMapControl1.get_Layer(0);
            IFeature feature1 = featureLayer1.FeatureClass.GetFeature(0);
            IPolyline polyline1= (IPolyline)feature1.Shape;

            //IFeatureLayer featureLayer2 = (IFeatureLayer)axMapControl1.get_Layer(1);
            IFeature feature2 = featureLayer1.FeatureClass.GetFeature(1);
            IPolyline polyline2 = (IPolyline)feature2.Shape;

            IRelationalOperator ro = (IRelationalOperator)polyline1;


            MessageBox.Show(ro.Relation(polyline2, "RELATE(G1, G2, '********T')").ToString());
        }

        private void returnDistanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer1 = (IFeatureLayer)axMapControl1.get_Layer(0);
            IFeature feature1 = featureLayer1.FeatureClass.GetFeature(0);
            IPolyline polyline1 = (IPolyline)feature1.Shape;

            //IFeatureLayer featureLayer2 = (IFeatureLayer)axMapControl1.get_Layer(1);
            IFeature feature2 = featureLayer1.FeatureClass.GetFeature(1);
            IPolyline polyline2 = (IPolyline)feature2.Shape;

            IProximityOperator po = (IProximityOperator)polyline1;
            
            MessageBox.Show(po.ReturnDistance(polyline2).ToString());
        }

        private void returnNearestPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer1 = (IFeatureLayer)axMapControl1.get_Layer(0);
            IFeature feature1 = featureLayer1.FeatureClass.GetFeature(0);
            IPoint point1 = new ESRI.ArcGIS.Geometry.Point();
            point1 = (IPoint)feature1.Shape;

            IFeatureLayer featureLayer2 = (IFeatureLayer)axMapControl1.get_Layer(1);
            IFeature feature2 = featureLayer2.FeatureClass.GetFeature(0);
            IPolyline polyline2 = (IPolyline)feature2.Shape;

            IProximityOperator po = (IProximityOperator)polyline2;
            IPoint point = po.ReturnNearestPoint(point1, esriSegmentExtension.esriNoExtension);

            IGraphicsContainer gc = (IGraphicsContainer)axMapControl1.Map;
            IElement element = new MarkerElement();

            element.Geometry = point;
            gc.AddElement(element, 0);
            axMapControl1.ActiveView.Refresh();
        }




    }
}
