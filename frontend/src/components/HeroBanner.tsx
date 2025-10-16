import { Button, Typography } from "antd";
import { Link } from "react-router-dom";
import styles from "./HeroBanner.module.css";

const { Title, Paragraph, Text } = Typography;

export const HeroBanner = () => {
  return (
    <section className={styles.hero}>
      <div className={styles.copy}>
        <Title level={1} className={styles.title}>
          Mang âm nhạc indie <span>đến người nghe toàn cầu</span>
        </Title>
        <Paragraph className={styles.subtitle}>
          MusicPlay giúp các nhạc sĩ mới tự tin phát hành, quảng bá và kiếm tiền từ bản nhạc của mình với hệ thống đề xuất thông minh, SEO tối ưu và cộng đồng yêu nhạc sẵn sàng lắng nghe.
        </Paragraph>
        <div className={styles.actions}>
          <Button type="primary" size="large" shape="round" className={styles.primaryAction}>
            <Link to="/upload">Tải track đầu tiên</Link>
          </Button>
          <Button size="large" shape="round" ghost>
            <Link to="/discover">Khám phá nghệ sĩ mới</Link>
          </Button>
        </div>
        <div className={styles.meta}>
          <Text strong>SEO tối ưu · Streaming mượt với 206 · Gợi ý playlist cá nhân hóa</Text>
        </div>
      </div>
      <div className={styles.visual}>
        <div className={styles.waveform}>
          {[...Array(24)].map((_, index) => (
            <span key={index} style={{ animationDelay: `${index * 120}ms` }} />
          ))}
        </div>
      </div>
    </section>
  );
};
