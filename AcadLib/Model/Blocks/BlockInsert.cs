﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AcadLib.Blocks
{
    public static class BlockInsert
    {
        // Файл шаблонов блоков
        internal static string fileCommonBlocks = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder, @"Blocks\Блоки-оформления.dwg");

        /// <summary>
        /// Вставка общего блока из файла Блоки-Оформления.
        /// Визуальная вставка с помошью Jig
        /// </summary>        
        public static ObjectId InsertCommonBlock(string blName, Database db)
        {
            // Выбор и вставка блока                 
            Block.CopyBlockFromExternalDrawing(blName, fileCommonBlocks, db, DuplicateRecordCloning.Ignore);
            return Insert(blName);
        }

        /// <summary>
        /// Вставка блока в чертеж - интерактивная (BlockInsertJig)
        /// </summary>        
        public static ObjectId Insert(string blName, Layers.LayerInfo layer, List<Property> props)
        {
            ObjectId idBlRefInsert;
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return ObjectId.Null;
            var db = doc.Database;
            var ed = doc.Editor;
            using (doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable) t.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (!bt.Has(blName))
                {
                    throw new Exception("Блок не определен в чертеже " + blName);
                }

                var idBlBtr = bt[blName];
                var pt = Point3d.Origin;
                var br = new BlockReference(pt, idBlBtr);
                br.SetDatabaseDefaults();
                if (layer != null)
                {
                    Layers.LayerExt.CheckLayerState(layer);
                    br.Layer = layer.Name;
                }

                var spaceBtr = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                idBlRefInsert = spaceBtr.AppendEntity(br);
                t.AddNewlyCreatedDBObject(br, true);

                if (props != null && br.IsDynamicBlock)
                {
                    foreach (DynamicBlockReferenceProperty item in br.DynamicBlockReferencePropertyCollection)
                    {
                        var prop = props.FirstOrDefault(p => p.Name.Equals(item.PropertyName, StringComparison.OrdinalIgnoreCase));
                        if (prop != null)
                        {
                            try
                            {
                                item.Value = prop.Value;
                            }
                            catch (Exception ex)
                            {
                                Logger.Log.Error(ex, $"Ошибка типа значения для дин параметра '{item.PropertyName}' " +
                                $"при вставке блока '{blName}': тип устанавливаемого значение '{prop.Value.GetType()}', " +
                                $"а должен быть тип '{item.UnitsType}'");
                            }
                        }
                    }
                }

                // jig
                var entJig = new Jigs.BlockInsertJig(br);
                var pr = ed.Drag(entJig);
                if (pr.Status == PromptStatus.OK)
                {
                    var btrBl = (BlockTableRecord) t.GetObject(idBlBtr, OpenMode.ForRead);
                    if (btrBl.HasAttributeDefinitions)
                        AddAttributes(br, btrBl, t);
                }
                else
                {
                    br.Erase();
                    idBlRefInsert = ObjectId.Null;
                }
                t.Commit();
            }
            return idBlRefInsert;
        }

        public static ObjectId Insert(string blName, Layers.LayerInfo layer)
        {
            return Insert(blName, layer, null);
        }

        public static ObjectId Insert(string blName, string layer)
        {
            var layerInfo = new Layers.LayerInfo(layer);
            return Insert(blName, layerInfo);
        }

        public static ObjectId Insert(string blName)
        {
            Layers.LayerInfo layer = null;
            return Insert(blName, layer);
        }

        /// <summary>
        /// Вставка вхождения блока
        /// </summary>
        /// <param name="blName">Имя блока</param>
        /// <param name="pt">Точка вставки</param>
        /// <param name="owner">Контейнер</param>        
        /// <param name="t"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        [NotNull]
        public static BlockReference InsertBlockRef(string blName, Point3d pt, [NotNull] BlockTableRecord owner, [NotNull] Transaction t, double scale = 1)
        {
            var db = owner.Database;
            var bt = (BlockTable) db.BlockTableId.GetObject(OpenMode.ForRead);
            var btr = (BlockTableRecord) bt[blName].GetObject(OpenMode.ForRead);
            var blRef = new BlockReference(pt, btr.Id)
            {
                Position = pt
            };
            if (blRef.Annotative == AnnotativeStates.True)
            {
                // Установка аннотативного масштаба
                blRef.AddContext(db.Cannoscale);
            }
            else if (scale != 1)
            {
                blRef.TransformBy(Matrix3d.Scaling(scale, pt));
            }
            blRef.SetDatabaseDefaults();
            owner.AppendEntity(blRef);
            t.AddNewlyCreatedDBObject(blRef, true);
            AddAttributes(blRef, btr, t);
            return blRef;
        }

        /// <summary>
        /// Добавление атрибутов к вставке блока
        /// </summary>        
        public static void AddAttributes(BlockReference blRef, [NotNull] BlockTableRecord btrBl, Transaction t)
        {
            foreach (var idEnt in btrBl)
            {
                if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
                {
                    var atrDef = t.GetObject(idEnt, OpenMode.ForRead) as AttributeDefinition;
                    if (!atrDef.Constant)
                    {
                        using (var atrRef = new AttributeReference())
                        {
                            atrRef.SetAttributeFromBlock(atrDef, blRef.BlockTransform);
                            //atrRef.TextString = atrDef.TextString;
                            blRef.AttributeCollection.AppendAttribute(atrRef);
                            t.AddNewlyCreatedDBObject(atrRef, true);
                        }
                    }
                }
            }
        }
    }
}