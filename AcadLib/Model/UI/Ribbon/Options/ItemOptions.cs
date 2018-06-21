namespace AcadLib.UI.Ribbon.Options
{
    using System.Collections.Generic;
    using Autodesk.Private.Windows;
    using Newtonsoft.Json;

    /// <summary>
    /// ��������� �������� �����
    /// </summary>
    public class ItemOptions
    {
        /// <summary>
        /// ������ �������� � ������������ ��������
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// ��������� ��������
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// ������ �� �������
        /// </summary>
        [JsonIgnore]
        public IRibbonContentUid Item { get; set; }

        /// <summary>
        /// ��������� ��������
        /// </summary>
        public List<ItemOptions> Items { get; set; } = new List<ItemOptions>();

        /// <summary>
        /// ��� ��������
        /// </summary>
        public string UID { get; set; }
    }
}