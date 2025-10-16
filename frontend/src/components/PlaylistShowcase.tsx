import { Card, Typography, Avatar, List } from "antd";
import { Link } from "react-router-dom";
import { CustomerServiceOutlined } from "@ant-design/icons";
import styles from "./PlaylistShowcase.module.css";

const { Title, Text } = Typography;

export interface PlaylistShowcaseProps {
  title: string;
  description?: string;
  coverUrl?: string;
  tracks: Array<{
    id: string;
    title: string;
    artistName: string;
    streamUrl: string;
  }>;
}

export const PlaylistShowcase = ({ title, description, coverUrl, tracks }: PlaylistShowcaseProps) => {
  return (
    <Card className={styles.card}>
      <div className={styles.header}>
        <div className={styles.cover}>
          {coverUrl ? (
            <img src={coverUrl} alt={title} />
          ) : (
            <div className={styles.iconWrap}>
              <CustomerServiceOutlined />
            </div>
          )}
        </div>
        <div>
          <Title level={3}>{title}</Title>
          {description && <Text className={styles.description}>{description}</Text>}
          <Link to="/discover" className={styles.link}>
            Nghe toàn bộ playlist
          </Link>
        </div>
      </div>
      <List
        className={styles.list}
        itemLayout="horizontal"
        dataSource={tracks.slice(0, 4)}
        renderItem={(track) => (
          <List.Item
            actions={[
              <a key="play" href={track.streamUrl} className={styles.playLink}>
                Nghe
              </a>
            ]}
          >
            <List.Item.Meta
              avatar={<Avatar shape="square" icon={<CustomerServiceOutlined />} />}
              title={track.title}
              description={track.artistName}
            />
          </List.Item>
        )}
      />
    </Card>
  );
};
