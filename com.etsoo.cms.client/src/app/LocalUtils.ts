import { UserAvatarEditorProps } from '@etsoo/materialui';
import { ArticleLogoDto } from '../api/dto/article/ArticleLogoDto';
import { ArticleQueryDto } from '../api/dto/article/ArticleQueryDto';
import { ArticleViewDto } from '../api/dto/article/ArticleViewDto';
import { TabDto } from '../api/dto/tab/TabDto';
import { TabLogoDto } from '../api/dto/tab/TabLogoDto';

/**
 * Local utilities
 */
export namespace LocalUtils {
  /**
   * Create article logo state
   * @param item Tab item
   * @returns Result
   */
  export function createLogoState(
    item: ArticleQueryDto | ArticleViewDto
  ): ArticleLogoDto {
    const { id, title, logo } = item;
    return { id, title, logo };
  }

  /**
   * Create tab logo state
   * @param item Tab item
   * @returns Result
   */
  export function createTabLogoState(item: TabDto): TabLogoDto {
    const { id, name, logo } = item;
    return { id, name, logo };
  }

  /**
   * Format editor size
   * @param size Size
   * @returns Result
   */
  export function formatEditorSize(
    size: [number, number]
  ): Partial<UserAvatarEditorProps> {
    let [width, height] = size;
    const maxWidth = width;
    const threshold = 800;

    if (width >= threshold) {
      height = Math.round((height * threshold) / width);
      width = threshold;
    }

    return {
      width,
      height,
      maxWidth
    };
  }
}
