import { Statistic, Row, Col } from "antd";
import styles from "./StatsBar.module.css";

export const StatsBar = () => (
  <Row gutter={[24, 24]} className={styles.row}>
    <Col xs={12} md={6}>
      <Statistic title="Nghệ sĩ indie" value={3200} suffix="+" valueStyle={{ color: "#f72585" }} />
    </Col>
    <Col xs={12} md={6}>
      <Statistic title="Track được stream" value={84000} suffix="+" valueStyle={{ color: "#32d4ff" }} />
    </Col>
    <Col xs={12} md={6}>
      <Statistic title="Người nghe hoạt động" value={1.2} suffix="M" valueStyle={{ color: "#8a4fff" }} precision={1} />
    </Col>
    <Col xs={12} md={6}>
      <Statistic title="Playlist do cộng đồng tạo" value={5600} suffix="+" valueStyle={{ color: "#ffd166" }} />
    </Col>
  </Row>
);
