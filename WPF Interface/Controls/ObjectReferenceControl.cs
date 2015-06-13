using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Interface.Controls
{
    public class ObjectReferenceControl : ComboBox
    {
        private ObjectReferenceSelector m_popup;

        static ObjectReferenceControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ObjectReferenceControl), new FrameworkPropertyMetadata(typeof(ObjectReferenceControl)));
        }

        public override void OnApplyTemplate()
        {
            if(m_popup == null)
            {
                m_popup = new ObjectReferenceSelector();
            }

            base.OnApplyTemplate();
        }
    }
}
