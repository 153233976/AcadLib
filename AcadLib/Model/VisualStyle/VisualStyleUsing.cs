﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using JetBrains.Annotations;

namespace AcadLib.VisualStyle
{
    public class VisualStyleUsing : IDisposable
    {
        private readonly Document doc;
        private readonly VisualStyleType previousStyle;

        public VisualStyleUsing([NotNull] Document doc, VisualStyleType style)
        {
            previousStyle = doc.Database.GetActiveVisualStyle();
            doc.SetActiveVisualStyle(style);
            this.doc = doc;
        }

        public void Dispose()
        {
            doc.SetActiveVisualStyle(previousStyle);
        }
    }
}
