/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace XtRay.Windows
{
    public class PercentToHeatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var percent = (double)value / 100.0;
                var r = percent < 0.5 ? percent * 2 * 255 : 255;
                var g = percent > 0.5 ? (percent - 1) * 2 * 2.55 : 255;
                return new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, 0));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
