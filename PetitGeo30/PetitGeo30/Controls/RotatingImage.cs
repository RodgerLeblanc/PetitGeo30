using System;
using Xamarin.Forms;

namespace PetitGeo30.Controls
{
    public class RotatingImage : Image
    {
        public static readonly BindableProperty DegreeProperty =
            BindableProperty.Create(
                nameof(Degree),
                typeof(double),
                typeof(RotatingImage),
                0.0,
                BindingMode.OneWay,
                null,
                (b, o, n) => OnDegreeChanged(b, o, n));

        public double Degree
        {
            get { return (double)GetValue(DegreeProperty); }
            set { SetValue(DegreeProperty, value); }
        }

        public static readonly BindableProperty RotationLengthProperty =
            BindableProperty.Create(
                nameof(RotationLength),
                typeof(int),
                typeof(RotatingImage),
                250);

        public int RotationLength
        {
            get { return (int)GetValue(RotationLengthProperty); }
            set { SetValue(RotationLengthProperty, value); }
        }

        private static async void OnDegreeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RotatingImage image)
            {
                double oldDegree = (double)oldValue;
                double newDegree = (double)newValue;
                double difference = (newDegree % 360) - (oldDegree % 360);

                if (difference < -180)
                    difference += 360;
                else if (difference > 180)
                    difference -= 360;

                await image.RotateTo(oldDegree + difference, (uint)Math.Abs(image.RotationLength), Easing.Linear);
            }
        }
    }
}
