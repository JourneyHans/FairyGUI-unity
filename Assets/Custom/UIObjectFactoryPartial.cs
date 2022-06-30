namespace FairyGUI {
    public partial class UIObjectFactory {
        public static GObject NewObject(ObjectType type) {
            Stats.LatestObjectCreation++;

            switch (type) {
                case ObjectType.Image:
                    return new GImage();

                case ObjectType.MovieClip:
                    return new GMovieClip();

                case ObjectType.Component:
                    return new GComponent();

                case ObjectType.Text:
                    return new GTextFieldExtension();

                case ObjectType.RichText:
                    return new GRichTextFieldExtension();

                case ObjectType.InputText:
                    return new GTextInput();

                case ObjectType.Group:
                    return new GGroup();

                case ObjectType.List:
                    return new GList();

                case ObjectType.Graph:
                    return new GGraph();

                case ObjectType.Loader:
                    if (loaderCreator != null)
                        return loaderCreator();
                    else
                        return new GLoader();

                case ObjectType.Button:
                    return new GButton();

                case ObjectType.Label:
                    return new GLabel();

                case ObjectType.ProgressBar:
                    return new GProgressBar();

                case ObjectType.Slider:
                    return new GSlider();

                case ObjectType.ScrollBar:
                    return new GScrollBar();

                case ObjectType.ComboBox:
                    return new GComboBoxExtension();

                case ObjectType.Tree:
                    return new GTree();

                case ObjectType.Loader3D:
                    return new GLoader3D();

                default:
                    return null;
            }
        }
    }
}