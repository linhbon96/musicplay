import { PlayCircleFilled } from "@ant-design/icons";
import styles from "./Logo.module.css";

export const Logo = () => (
  <div className={styles.wrapper}>
    <div className={styles.iconWrap}>
      <PlayCircleFilled className={styles.icon} />
    </div>
    <div>
      <span className={styles.brand}>MusicPlay</span>
      <span className={styles.tagline}>Amplify new voices</span>
    </div>
  </div>
);
