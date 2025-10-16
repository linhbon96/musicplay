import { Card, Typography } from "antd";
import { PlayCircleFilled, SoundOutlined } from "@ant-design/icons";
import styles from "./TrackCard.module.css";

const { Title, Text } = Typography;

export interface TrackCardProps {
  title: string;
  artistName: string;
  description?: string;
  streamUrl: string;
  coverUrl?: string;
  featured?: boolean;
}

export const TrackCard = ({ title, artistName, description, streamUrl, coverUrl, featured }: TrackCardProps) => {
  return (
    <Card
      className={styles.card}
      cover={
        <div className={styles.cover}>
          {coverUrl ? <img src={coverUrl} alt={title} /> : <SoundOutlined className={styles.placeholder} />}
          {featured && <span className={styles.badge}>Nổi bật</span>}
        </div>
      }
      actions={[
        <a href={streamUrl} key="play" className={styles.playAction}>
          <PlayCircleFilled /> Nghe ngay
        </a>
      ]}
    >
      <Title level={4} className={styles.title}>
        {title}
      </Title>
      <Text className={styles.artist}>{artistName}</Text>
      {description && <Text className={styles.description}>{description}</Text>}
    </Card>
  );
};
