import type { ThemeConfig } from "antd";
import { cyan, geekblue, purple } from "@ant-design/colors";

export const primaryGradient = "linear-gradient(135deg, #1f1633 0%, #2b1940 45%, #4c1d95 100%)";

export const themeConfig: ThemeConfig = {
  token: {
    colorPrimary: "#6930c3",
    colorInfo: "#8a4fff",
    colorBgBase: "#0e0918",
    colorTextBase: "#f7f5ff",
    fontFamily: "'Poppins', 'Segoe UI', sans-serif",
    borderRadius: 16
  },
  components: {
    Layout: {
      headerBg: "#0e0918",
      bodyBg: "#0e0918"
    },
    Button: {
      colorPrimaryBg: "#8a4fff",
      colorPrimaryHover: geekblue[4],
      colorPrimaryActive: geekblue[6]
    },
    Card: {
      colorBgContainer: "rgba(18, 12, 32, 0.85)",
      colorBorderSecondary: "rgba(255, 255, 255, 0.06)",
      boxShadow: "0 24px 60px rgba(26, 14, 46, 0.45)"
    },
    Menu: {
      itemColor: "rgba(247, 245, 255, 0.75)",
      itemHoverColor: cyan[2],
      itemSelectedColor: "#ffffff",
      itemSelectedBg: "rgba(138, 79, 255, 0.2)",
      subMenuItemBg: "transparent"
    }
  }
};

export const accentColors = {
  aurora: "#32d4ff",
  twilight: "#f72585"
};
