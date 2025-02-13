namespace Tickblaze.Scripts.Drawings;

public partial class VolumeProfile
{
	public interface ISettings
	{
		public SourceDataType SourceData { get; set; }
		public RowsLayoutType RowsLayout { get; set; }
		public int RowsSize { get; set; }
		public double RowsWidthPercent { get; set; }
		public PlacementType RowsPlacement { get; set; }
		public double ValueAreaPercent { get; set; }
		public Color ValueAreaColor { get; set; }
		public Color ValueAreaAboveColor { get; set; }
		public Color ValueAreaBelowColor { get; set; }
		public bool BoxVisible { get; set; }
		public Color BoxLineColor { get; set; }
		public int BoxLineThickness { get; set; }
		public LineStyle BoxLineStyle { get; set; }
		public bool VahLineVisible { get; set; }
		public Color VahLineColor { get; set; }
		public int VahLineThickness { get; set; }
		public LineStyle VahLineStyle { get; set; }
		public bool ValLineVisible { get; set; }
		public Color ValLineColor { get; set; }
		public int ValLineThickness { get; set; }
		public LineStyle ValLineStyle { get; set; }
		public bool PocLineVisible { get; set; }
		public Color PocLineColor { get; set; }
		public int PocLineThickness { get; set; }
		public LineStyle PocLineStyle { get; set; }
		public bool ShowPrices { get; set; }
		public Font Font { get; set; }
		public bool VwapEnabled { get; set; }
		public Color VwapLineColor { get; set; }
		public int VwapLineThickness { get; set; }
		public LineStyle VwapLineStyle { get; set; }
	}
}