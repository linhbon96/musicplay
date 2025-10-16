import { Layout, Menu } from "antd";
import { Outlet, useLocation, useNavigate } from "react-router-dom";
import {
  CustomerServiceOutlined,
  FireOutlined,
  HomeOutlined,
  UploadOutlined
} from "@ant-design/icons";
import { Logo } from "../components/Logo";
import { primaryGradient } from "../theme";
import styles from "./AppLayout.module.css";

const { Header, Content, Footer } = Layout;

export const AppLayout = () => {
  const location = useLocation();
  const navigate = useNavigate();

  const onMenuClick = ({ key }: { key: string }) => {
    navigate(key);
  };

  return (
    <Layout className={styles.container}>
      <Header className={styles.header}>
        <div className={styles.logoArea}>
          <Logo />
        </div>
        <Menu
          mode="horizontal"
          selectedKeys={[location.pathname]}
          items={[
            { key: "/", icon: <HomeOutlined />, label: "Trang chủ" },
            { key: "/discover", icon: <FireOutlined />, label: "Khám phá" },
            { key: "/upload", icon: <UploadOutlined />, label: "Tải nhạc" },
            { key: "/admin", icon: <CustomerServiceOutlined />, label: "Quản trị" }
          ]}
          onClick={onMenuClick}
        />
      </Header>
      <Content className={styles.content}>
        <Outlet />
      </Content>
      <Footer className={styles.footer}>
        © {new Date().getFullYear()} MusicPlay · Nền tảng kết nối nhạc sĩ mới với người nghe toàn cầu.
      </Footer>
      <div className={styles.backgroundGlow} style={{ backgroundImage: primaryGradient }} />
    </Layout>
  );
};
