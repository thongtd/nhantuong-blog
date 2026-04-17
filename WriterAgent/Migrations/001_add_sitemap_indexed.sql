-- Add sitemap_indexed column to track which records are already in sitemap
ALTER TABLE blogs ADD COLUMN sitemap_indexed TINYINT(1) NOT NULL DEFAULT 0;
ALTER TABLE blog_tags ADD COLUMN sitemap_indexed TINYINT(1) NOT NULL DEFAULT 0;
ALTER TABLE blog_categories ADD COLUMN sitemap_indexed TINYINT(1) NOT NULL DEFAULT 0;

-- Mark all existing enabled records as already indexed
-- (because they are already in the current sitemap.xml)
UPDATE blogs SET sitemap_indexed = 1 WHERE enabled = 1 AND is_delete = 0;
UPDATE blog_tags SET sitemap_indexed = 1 WHERE enabled = 1 AND is_delete = 0;
UPDATE blog_categories SET sitemap_indexed = 1 WHERE enabled = 1 AND is_delete = 0;
