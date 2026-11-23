using UnityEngine;

namespace CalmWoods.UI
{
    // Optional tag that tells UIThemeApplier what an element is.
    // Without it: buttons are styled as Secondary, texts keep their size and get the font
    // and cream color, and other images are left alone.
    public class UIStyle : MonoBehaviour
    {
        public enum Role
        {
            Auto,
            Title,
            Heading,
            Body,
            Caption,
            PrimaryButton,
            SecondaryButton,
            Panel,
            Ignore
        }

        public Role role = Role.Auto;
    }
}
