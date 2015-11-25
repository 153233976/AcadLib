﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AcadLib.Layers
{
   public class LayerExt
   {
      /// <summary>
      /// Получение слоя.
      /// Если его нет в базе, то создается.      
      /// </summary>
      /// <param name="layerInfo">параметры слоя</param>
      /// <returns></returns>
      public static ObjectId GetLayerOrCreateNew(LayerInfo layerInfo)
      {
         Database db = HostApplicationServices.WorkingDatabase;
         // Если уже был создан слой, то возвращаем его. Опасно, т.к. перед повторным запуском команды покраски, могут удалить/переименовать слой марок.                  
         using (var t = db.TransactionManager.StartTransaction())
         {
            var lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            if (lt.Has(layerInfo.Name))
            {
               return lt[layerInfo.Name];
            }
            return CreateLayer(layerInfo, lt, t);
            t.Commit();
         }        
      }

      /// <summary>
      /// Создание слоя.
      /// Слоя не должно быть в таблице слоев.
      /// </summary>
      /// <param name="layerInfo">параметры слоя</param>
      /// <param name="lt">таблица слоев открытая для чтения. Выполняется UpgradeOpen и DowngradeOpen</param>
      public static ObjectId CreateLayer(LayerInfo layerInfo, LayerTable lt, Transaction t)
      {
         ObjectId idLayer = ObjectId.Null;
         // Если слоя нет, то он создается.            
         var newLayer = new LayerTableRecord();         
         newLayer.Name = layerInfo.Name;
         newLayer.Color = layerInfo.Color;
         newLayer.IsFrozen = layerInfo.IsFrozen;
         newLayer.IsLocked = layerInfo.IsLocked;
         newLayer.IsOff = layerInfo.IsOff;
         newLayer.IsPlottable = layerInfo.IsPlotable;
         if (!layerInfo.LinetypeObjectId.IsNull)
            newLayer.LinetypeObjectId = layerInfo.LinetypeObjectId;
         lt.UpgradeOpen();
         idLayer = lt.Add(newLayer);
         t.AddNewlyCreatedDBObject(newLayer, true);
         lt.DowngradeOpen();
         return idLayer;
      }

      /// <summary>
      /// Проверка блокировки слоя IsOff IsLocked IsFrozen.
      /// Если заблокировано - то разблокируется.
      /// Если слоя нет - то он создается с дефолтными параметрами.
      /// </summary>
      /// <param name="layers">Список слоев для проверкм в текущей рабочей базе</param>
      public static void CheckLayerState(string[] layers)
      {
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            foreach (var layer in layers)
            {
               if (lt.Has(layer))
               {
                  var lay = lt[layer].GetObject(OpenMode.ForRead) as LayerTableRecord;
                  if (lay.IsLocked && lay.IsOff && lay.IsFrozen)
                  {
                     lay.UpgradeOpen();
                     if (lay.IsOff)
                     {
                        lay.IsOff = false;
                     }
                     if (lay.IsLocked)
                     {
                        lay.IsLocked = false;
                     }
                     if (lay.IsFrozen)
                     {
                        lay.IsFrozen = false;
                     }
                  }
               }
               else
               {
                  CreateLayer(new LayerInfo(layer), lt, t);
               }
            }
            t.Commit();
         }
      }

      public static void CheckLayerState(string layer)
      {
         string[] layers = new string[1];
         layers[0] = layer;
         CheckLayerState(layers);
      }
   }
}
